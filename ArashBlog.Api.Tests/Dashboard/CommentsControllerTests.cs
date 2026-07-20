using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArashBlog.Api.Tests.Dashboard;

public class CommentsControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task<Comment> SeedCommentAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var suffix = Guid.NewGuid().ToString("N");
        var author = new ApplicationUser { UserName = $"comment-author-{suffix}", Email = $"comment-author-{suffix}@example.com" };
        await userManager.CreateAsync(author, "Sup3r-Secret!");

        var post = new Post
        {
            TitleFa = "پست", TitleCkb = "پۆست", Slug = $"post-{suffix}",
            BodyFa = "<p>x</p>", BodyCkb = "<p>x</p>",
            Status = PostStatus.Published, PublishedAt = DateTimeOffset.UtcNow, AuthorId = author.Id,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var comment = new Comment { PostId = post.Id, Name = "Reader", Email = "reader@example.com", Body = "hi", IsApproved = false };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return comment;
    }

    [Fact]
    public async Task Toggle_flips_approval_and_delete_removes_it()
    {
        var comment = await SeedCommentAsync();
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "moderator-1");

        var listResponse = await client.GetFromJsonAsync<List<DashboardCommentDto>>("/api/dashboard/comments");
        Assert.Contains(listResponse!, c => c.Id == comment.Id && !c.IsApproved);

        var toggleResponse = await client.PostAsync($"/api/dashboard/comments/{comment.Id}/toggle", null);
        Assert.Equal(HttpStatusCode.NoContent, toggleResponse.StatusCode);

        var afterToggle = await client.GetFromJsonAsync<List<DashboardCommentDto>>("/api/dashboard/comments");
        Assert.Contains(afterToggle!, c => c.Id == comment.Id && c.IsApproved);

        var deleteResponse = await client.DeleteAsync($"/api/dashboard/comments/{comment.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetFromJsonAsync<List<DashboardCommentDto>>("/api/dashboard/comments");
        Assert.DoesNotContain(afterDelete!, c => c.Id == comment.Id);
    }
}
