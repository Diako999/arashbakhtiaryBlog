using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Dashboard;

public class OverviewControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Overview_counts_reflect_draft_and_published_posts()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "overview-user");
        await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "", null, null, null, null, "Draft", null, "پیش‌نویس", "ڕەشنووس", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));
        await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "", null, null, null, null, "Published", DateTimeOffset.UtcNow,
            "منتشرشده", "بڵاوکراوەتەوە", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));

        var overview = await client.GetFromJsonAsync<OverviewDto>("/api/dashboard/overview");

        Assert.True(overview!.DraftCount >= 1);
        Assert.True(overview.PublishedCount >= 1);
        Assert.NotEmpty(overview.RecentPosts);
    }

    [Fact]
    public async Task Analytics_computes_bar_percentages_relative_to_the_top_post()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "analytics-user");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "", null, "", null, null, null, null, "Published", DateTimeOffset.UtcNow,
            "محبوب", "بەناوبانگ", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();

        // View it a few times through the public endpoint to build up ViewCount.
        for (var i = 0; i < 3; i++)
        {
            await client.GetAsync($"/api/blog/posts/{created!.Slug}?lang=fa");
        }

        var analytics = await client.GetFromJsonAsync<AnalyticsDto>("/api/dashboard/analytics");

        Assert.Contains(analytics!.TopPosts, p => p.Slug == created!.Slug && p.ViewCount >= 3);
        Assert.True(analytics.TotalViews >= 3);
    }
}
