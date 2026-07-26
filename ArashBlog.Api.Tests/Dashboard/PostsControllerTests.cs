using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Dashboard;

public class PostsControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Create_category_then_create_post_referencing_it()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "content-editor-1");

        var categoryResponse = await client.PostAsJsonAsync(
            "/api/dashboard/categories", new UpsertCategoryRequest("", "دسته آزمایشی", "پۆلی تاقیکردنەوە"));
        var category = await categoryResponse.Content.ReadFromJsonAsync<DashboardCategoryDto>();
        Assert.Equal(HttpStatusCode.OK, categoryResponse.StatusCode);

        var postResponse = await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", category!.Id, "تگ یک, تگ دو", null, null, null, null, "Published", DateTimeOffset.UtcNow,
            "عنوان جدید", "ناونیشانی نوێ", "خلاصه", "کورتە", "<p>بدنه</p>", "<p>ناوەڕۆک</p>",
            "", "", "", ""));
        var post = await postResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Equal(category.Id, post!.CategoryId);
        Assert.Contains("تگ یک", post.Tags);
        Assert.Contains("تگ دو", post.Tags);
        Assert.NotEmpty(post.Slug);
    }

    [Fact]
    public async Task Create_rejects_duplicate_slug()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "content-editor-2");
        var request = new UpsertPostRequest(
            "fixed-slug", null, "", null, null, null, null, "Draft", null,
            "عنوان اول", "ناونیشان", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "");

        var first = await client.PostAsJsonAsync("/api/dashboard/posts", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/dashboard/posts", request with { TitleFa = "عنوان دوم" });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Update_sanitizes_body_and_replaces_tags()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "content-editor-3");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "old-tag", null, null, null, null, "Draft", null,
            "اولیه", "سەرەتایی", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/dashboard/posts/{created!.Id}", new UpsertPostRequest(
            created.Slug, null, "new-tag", null, null, null, null, "Published", DateTimeOffset.UtcNow,
            "بروزرسانی‌شده", "نوێکراوە", "", "", "<p>ok</p><script>alert(1)</script>", "<p>ok</p>", "", "", "", ""));
        var updated = await updateResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.DoesNotContain("<script", updated!.BodyFa);
        Assert.DoesNotContain("old-tag", updated.Tags);
        Assert.Contains("new-tag", updated.Tags);

        var publicDetail = await client.GetFromJsonAsync<Features.Blog.PostDetailDto>($"/api/blog/posts/{created.Slug}?lang=fa");
        Assert.Equal("بروزرسانی‌شده", publicDetail!.Title);
    }

    [Fact]
    public async Task Delete_removes_the_post()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "content-editor-4");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "", null, null, null, null, "Draft", null, "حذف‌شدنی", "سڕاوە", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();

        var deleteResponse = await client.DeleteAsync($"/api/dashboard/posts/{created!.Id}");
        var getResponse = await client.GetAsync($"/api/dashboard/posts/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_accepts_valid_custom_colors_and_they_reach_the_public_summary()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "content-editor-5");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "", null, "#111111", "#eeeeee", "#ff0000", "Published", DateTimeOffset.UtcNow,
            "رنگی", "ڕەنگاوڕەنگ", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal("#111111", created!.BgColor);

        var publicPosts = await client.GetFromJsonAsync<Features.Blog.PostListResponse>("/api/blog/posts?lang=fa");
        var publicPost = publicPosts!.Items.Single(p => p.Slug == created.Slug);
        Assert.Equal("#111111", publicPost.BgColor);
        Assert.Equal("#eeeeee", publicPost.TextColor);
        Assert.Equal("#ff0000", publicPost.AccentColor);
    }

    [Fact]
    public async Task Create_rejects_an_invalid_custom_color()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "content-editor-6");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "", null, "not-a-color", null, null, "Draft", null,
            "نامعتبر", "نادروست", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }

    [Fact]
    public async Task List_is_denied_without_2fa_verification()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/posts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
