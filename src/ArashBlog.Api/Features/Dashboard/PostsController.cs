using System.Text.RegularExpressions;
using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's PostDashboardListView/PostCreateView/
// PostUpdateView/PostDeleteView/CategoryCreateView/CategoryUpdateView.
// Cover images stay a plain URL field for now — full upload validation
// (real-content-type sniffing, size caps) is a separate concern from CRUD
// wiring and is deferred rather than half-built here.
[ApiController]
[Route("api/dashboard")]
[RequireVerifiedTwoFactor]
public partial class PostsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [GeneratedRegex(@"^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();

    // Unlike SettingsController's colors (always present on a required
    // ThemeConfig row), post colors are optional — only validate when the
    // admin actually supplied a value.
    private static bool IsValidOptionalColor(string? value) => value is null || HexColorRegex().IsMatch(value);
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<DashboardCategoryDto>>> Categories()
    {
        var categories = await db.Categories.OrderBy(c => c.NameFa).ToListAsync();
        return Ok(categories.Select(c => new DashboardCategoryDto(c.Id, c.Slug, c.NameFa, c.NameCkb)).ToList());
    }

    [HttpPost("categories")]
    public async Task<ActionResult<DashboardCategoryDto>> CreateCategory(UpsertCategoryRequest request)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugifier.Slugify(request.NameFa) : request.Slug;
        if (await db.Categories.AnyAsync(c => c.Slug == slug))
        {
            return Conflict(new { error = "slug_taken" });
        }

        var category = new Category { NameFa = request.NameFa, NameCkb = request.NameCkb, Slug = slug };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return Ok(new DashboardCategoryDto(category.Id, category.Slug, category.NameFa, category.NameCkb));
    }

    [HttpPut("categories/{id:int}")]
    public async Task<ActionResult<DashboardCategoryDto>> UpdateCategory(int id, UpsertCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        if (await db.Categories.AnyAsync(c => c.Slug == request.Slug && c.Id != id))
        {
            return Conflict(new { error = "slug_taken" });
        }

        category.NameFa = request.NameFa;
        category.NameCkb = request.NameCkb;
        category.Slug = request.Slug;
        await db.SaveChangesAsync();

        return Ok(new DashboardCategoryDto(category.Id, category.Slug, category.NameFa, category.NameCkb));
    }

    [HttpGet("posts")]
    public async Task<ActionResult<DashboardPostListResponse>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Posts.Include(p => p.Category).OrderByDescending(p => p.CreatedAt);
        var totalCount = await query.CountAsync();
        var posts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = posts.Select(p => new DashboardPostListItemDto(
            p.Id, p.TitleFa, p.Slug, p.Category?.NameFa, p.Status.ToString(), p.CreatedAt)).ToList();

        return Ok(new DashboardPostListResponse(items, page, pageSize, totalCount));
    }

    [HttpGet("posts/{id:int}")]
    public async Task<ActionResult<DashboardPostDetailDto>> Detail(int id)
    {
        var post = await db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        return Ok(ToDetailDto(post));
    }

    [HttpPost("posts")]
    public async Task<ActionResult<DashboardPostDetailDto>> Create(UpsertPostRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? await GenerateUniqueSlugAsync(request.TitleFa) : request.Slug;
        if (await db.Posts.AnyAsync(p => p.Slug == slug))
        {
            return Conflict(new { error = "slug_taken" });
        }

        if (!Enum.TryParse<PostStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new { error = "invalid_status" });
        }

        if (!IsValidOptionalColor(request.BgColor) || !IsValidOptionalColor(request.TextColor) || !IsValidOptionalColor(request.AccentColor))
        {
            return BadRequest(new { error = "invalid_color" });
        }

        var post = new Post
        {
            Slug = slug,
            CategoryId = request.CategoryId,
            CoverImageUrl = request.CoverImageUrl,
            BgColor = request.BgColor,
            TextColor = request.TextColor,
            AccentColor = request.AccentColor,
            Status = status,
            PublishedAt = request.PublishedAt,
            TitleFa = request.TitleFa,
            TitleCkb = request.TitleCkb,
            ExcerptFa = request.ExcerptFa,
            ExcerptCkb = request.ExcerptCkb,
            BodyFa = PostBodySanitizer.Sanitize(request.BodyFa),
            BodyCkb = PostBodySanitizer.Sanitize(request.BodyCkb),
            MetaTitleFa = request.MetaTitleFa,
            MetaTitleCkb = request.MetaTitleCkb,
            MetaDescriptionFa = request.MetaDescriptionFa,
            MetaDescriptionCkb = request.MetaDescriptionCkb,
            AuthorId = user.Id,
        };

        db.Posts.Add(post);
        await SyncTagsAsync(post, request.Tags);
        await db.SaveChangesAsync();

        return Ok(ToDetailDto(post));
    }

    [HttpPut("posts/{id:int}")]
    public async Task<ActionResult<DashboardPostDetailDto>> Update(int id, UpsertPostRequest request)
    {
        var post = await db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        if (await db.Posts.AnyAsync(p => p.Slug == request.Slug && p.Id != id))
        {
            return Conflict(new { error = "slug_taken" });
        }

        if (!Enum.TryParse<PostStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new { error = "invalid_status" });
        }

        if (!IsValidOptionalColor(request.BgColor) || !IsValidOptionalColor(request.TextColor) || !IsValidOptionalColor(request.AccentColor))
        {
            return BadRequest(new { error = "invalid_color" });
        }

        post.Slug = request.Slug;
        post.CategoryId = request.CategoryId;
        post.CoverImageUrl = request.CoverImageUrl;
        post.BgColor = request.BgColor;
        post.TextColor = request.TextColor;
        post.AccentColor = request.AccentColor;
        post.Status = status;
        post.PublishedAt = request.PublishedAt;
        post.TitleFa = request.TitleFa;
        post.TitleCkb = request.TitleCkb;
        post.ExcerptFa = request.ExcerptFa;
        post.ExcerptCkb = request.ExcerptCkb;
        post.BodyFa = PostBodySanitizer.Sanitize(request.BodyFa);
        post.BodyCkb = PostBodySanitizer.Sanitize(request.BodyCkb);
        post.MetaTitleFa = request.MetaTitleFa;
        post.MetaTitleCkb = request.MetaTitleCkb;
        post.MetaDescriptionFa = request.MetaDescriptionFa;
        post.MetaDescriptionCkb = request.MetaDescriptionCkb;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        await SyncTagsAsync(post, request.Tags);
        await db.SaveChangesAsync();

        return Ok(ToDetailDto(post));
    }

    [HttpDelete("posts/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null) return NotFound();

        db.Posts.Remove(post);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> GenerateUniqueSlugAsync(string titleFa)
    {
        var baseSlug = Slugifier.Slugify(titleFa);
        var slug = baseSlug;
        var counter = 1;
        while (await db.Posts.AnyAsync(p => p.Slug == slug))
        {
            counter += 1;
            slug = $"{baseSlug}-{counter}";
        }
        return slug;
    }

    // Free-text, comma-separated tag input (matches django-taggit's default
    // widget) — find-or-create each tag by its unicode-safe slug and sync
    // the post's tag set to exactly what was submitted.
    private async Task SyncTagsAsync(Post post, string tagsInput)
    {
        var names = tagsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();

        post.Tags.Clear();
        foreach (var name in names)
        {
            var slug = Slugifier.Slugify(name);
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
            if (tag is null)
            {
                tag = new Tag { Name = name, Slug = slug };
                db.Tags.Add(tag);
            }
            post.Tags.Add(tag);
        }
    }

    private static DashboardPostDetailDto ToDetailDto(Post p) => new(
        p.Id, p.Slug, p.CategoryId, string.Join(", ", p.Tags.Select(t => t.Name)), p.CoverImageUrl,
        p.BgColor, p.TextColor, p.AccentColor,
        p.Status.ToString(), p.PublishedAt, p.TitleFa, p.TitleCkb, p.ExcerptFa, p.ExcerptCkb, p.BodyFa, p.BodyCkb,
        p.MetaTitleFa, p.MetaTitleCkb, p.MetaDescriptionFa, p.MetaDescriptionCkb);
}
