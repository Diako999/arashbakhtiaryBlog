using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Blog;

// Mirrors apps/blog/{models,views}.py. Blog is the one section that's
// always live, never gated by the NavItem phased-rollout mechanism.
[ApiController]
[Route("api/blog")]
public class BlogController(ApplicationDbContext db) : ControllerBase
{
    private static string PickLang(string? lang) => lang == "ckb" ? "ckb" : "fa";

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Categories([FromQuery] string? lang)
    {
        var l = PickLang(lang);
        var categories = await db.Categories.OrderBy(c => c.NameFa).ToListAsync();
        return Ok(categories.Select(c => new CategoryDto(c.Slug, l == "ckb" ? c.NameCkb : c.NameFa)).ToList());
    }

    [HttpGet("posts")]
    public async Task<ActionResult<PostListResponse>> List(
        [FromQuery] string? lang,
        [FromQuery] string? category,
        [FromQuery] string? tag,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var l = PickLang(lang);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category != null && p.Category.Slug == category);

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(p => p.Tags.Any(t => t.Slug == tag));

        if (!string.IsNullOrWhiteSpace(q))
        {
            // Plain contains search across the active language's columns only
            // — same deliberate no-ranking/no-stemming scope as the Django
            // icontains search this replaces (see the MySQL-migration note
            // in the original project's CLAUDE.md).
            query = l == "ckb"
                ? query.Where(p => EF.Functions.Like(p.TitleCkb, $"%{q}%") ||
                                    EF.Functions.Like(p.ExcerptCkb, $"%{q}%") ||
                                    EF.Functions.Like(p.BodyCkb, $"%{q}%"))
                : query.Where(p => EF.Functions.Like(p.TitleFa, $"%{q}%") ||
                                    EF.Functions.Like(p.ExcerptFa, $"%{q}%") ||
                                    EF.Functions.Like(p.BodyFa, $"%{q}%"));
        }

        var totalCount = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = posts.Select(p => ToSummary(p, l)).ToList();
        return Ok(new PostListResponse(items, page, pageSize, totalCount));
    }

    [HttpGet("posts/{slug}")]
    public async Task<ActionResult<PostDetailDto>> Detail(string slug, [FromQuery] string? lang)
    {
        var l = PickLang(lang);
        var post = await db.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PostStatus.Published);

        if (post is null) return NotFound();

        post.ViewCount += 1;
        await db.SaveChangesAsync();

        var comments = post.Comments
            .Where(c => c.IsApproved)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.Name, c.Body, c.CreatedAt))
            .ToList();

        var dto = new PostDetailDto(
            post.Slug,
            l == "ckb" ? post.TitleCkb : post.TitleFa,
            l == "ckb" ? post.BodyCkb : post.BodyFa,
            l == "ckb" ? post.ExcerptCkb : post.ExcerptFa,
            post.CoverImageUrl,
            post.BgColor,
            post.TextColor,
            post.AccentColor,
            post.Category is null ? null : l == "ckb" ? post.Category.NameCkb : post.Category.NameFa,
            post.Category?.Slug,
            post.Tags.Select(t => t.Slug).ToList(),
            post.PublishedAt,
            post.Author.UserName ?? "",
            post.ViewCount,
            comments);

        return Ok(dto);
    }

    [HttpPost("posts/{slug}/comments")]
    [EnableRateLimiting("comments")]
    public async Task<IActionResult> CreateComment(string slug, CreateCommentRequest request)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PostStatus.Published);
        if (post is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { error = "invalid_comment" });
        }

        var comment = new Comment
        {
            PostId = post.Id,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Body = request.Body.Trim(),
            IsApproved = false,
        };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { message = "submitted" });
    }

    private static PostSummaryDto ToSummary(Post p, string lang) => new(
        p.Slug,
        lang == "ckb" ? p.TitleCkb : p.TitleFa,
        lang == "ckb" ? p.ExcerptCkb : p.ExcerptFa,
        p.CoverImageUrl,
        p.BgColor,
        p.TextColor,
        p.AccentColor,
        p.Category is null ? null : lang == "ckb" ? p.Category.NameCkb : p.Category.NameFa,
        p.Category?.Slug,
        p.Tags.Select(t => t.Slug).ToList(),
        p.PublishedAt,
        p.Author.UserName ?? "");
}
