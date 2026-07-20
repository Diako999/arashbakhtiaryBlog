using System.Linq;
using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Blog;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArashBlog.Api.Tests.Blog;

public class BlogEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task<Post> SeedPostAsync(
        string titleFa = "عنوان نمونه", string bodyFa = "<p>متن نمونه</p>", PostStatus status = PostStatus.Published)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var authorSuffix = Guid.NewGuid().ToString("N");
        var author = new ApplicationUser { UserName = $"author-{authorSuffix}", Email = $"author-{authorSuffix}@example.com" };
        var createResult = await userManager.CreateAsync(author, "Sup3r-Secret!");
        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var category = new Category { NameFa = "دسته", NameCkb = "پۆل", Slug = $"cat-{Guid.NewGuid():N}" };
        db.Categories.Add(category);

        var post = new Post
        {
            TitleFa = titleFa,
            TitleCkb = "ناونیشانی نموونە",
            Slug = $"{Slugifier.Slugify(titleFa)}-{Guid.NewGuid():N}",
            ExcerptFa = "خلاصه",
            ExcerptCkb = "کورتە",
            BodyFa = PostBodySanitizer.Sanitize(bodyFa),
            BodyCkb = PostBodySanitizer.Sanitize("<p>دەقی نموونە</p>"),
            Status = status,
            PublishedAt = status == PostStatus.Published ? DateTimeOffset.UtcNow : null,
            AuthorId = author.Id,
            CategoryId = category.Id,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        return post;
    }

    [Fact]
    public async Task List_returns_only_published_posts()
    {
        var published = await SeedPostAsync();
        await SeedPostAsync(titleFa: "پیش‌نویس پنهان", status: PostStatus.Draft);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blog/posts?lang=fa");
        var result = await response.Content.ReadFromJsonAsync<PostListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(result!.Items, p => p.Slug == published.Slug);
        Assert.DoesNotContain(result.Items, p => p.Title == "پیش‌نویس پنهان");
    }

    [Fact]
    public async Task Search_matches_only_the_active_language_columns()
    {
        var uniqueWord = $"یکتا{Guid.NewGuid():N}"[..12];
        await SeedPostAsync(titleFa: uniqueWord);
        var client = factory.CreateClient();
        var encodedWord = Uri.EscapeDataString(uniqueWord);

        var faResult = await client.GetFromJsonAsync<PostListResponse>($"/api/blog/posts?lang=fa&q={encodedWord}");
        var ckbResult = await client.GetFromJsonAsync<PostListResponse>($"/api/blog/posts?lang=ckb&q={encodedWord}");

        Assert.NotEmpty(faResult!.Items);
        Assert.Empty(ckbResult!.Items);
    }

    [Fact]
    public async Task Detail_increments_view_count_and_returns_sanitized_body()
    {
        var post = await SeedPostAsync(bodyFa: "<p>ok</p><script>alert(1)</script>");
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/blog/posts/{Uri.EscapeDataString(post.Slug)}?lang=fa");
        var detail = await response.Content.ReadFromJsonAsync<PostDetailDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("<script", detail!.BodyHtml);
        Assert.Equal(1, detail.ViewCount);
    }

    [Fact]
    public async Task Unknown_slug_returns_404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blog/posts/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Comment_submission_is_saved_unapproved_and_not_returned_in_detail()
    {
        var post = await SeedPostAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/blog/posts/{Uri.EscapeDataString(post.Slug)}/comments",
            new CreateCommentRequest("Reader", "reader@example.com", "Great post!"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var detail = await client.GetFromJsonAsync<PostDetailDto>($"/api/blog/posts/{Uri.EscapeDataString(post.Slug)}?lang=fa");
        Assert.Empty(detail!.Comments);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var saved = db.Comments.Single(c => c.PostId == post.Id);
        Assert.False(saved.IsApproved);
    }
}
