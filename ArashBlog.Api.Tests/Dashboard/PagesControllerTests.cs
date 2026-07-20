using System.Linq;
using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Dashboard;

public class PagesControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task List_excludes_blog_and_includes_the_phased_rollout_sections()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "pages-admin-1");

        var items = await client.GetFromJsonAsync<List<DashboardNavItemDto>>("/api/dashboard/nav-items");

        Assert.DoesNotContain(items!, i => i.Key == "blog");
        Assert.Contains(items!, i => i.Key == "offerings");
        Assert.Contains(items!, i => i.Key == "testimonials");
        Assert.Contains(items!, i => i.Key == "leads");
    }

    [Fact]
    public async Task Toggle_flips_visibility_and_affects_the_public_gate()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "pages-admin-2");
        var items = await client.GetFromJsonAsync<List<DashboardNavItemDto>>("/api/dashboard/nav-items");
        var offerings = items!.Single(i => i.Key == "offerings");
        var wasVisible = offerings.IsVisible;

        var toggleResponse = await client.PostAsync($"/api/dashboard/nav-items/{offerings.Id}/toggle", null);
        Assert.Equal(HttpStatusCode.NoContent, toggleResponse.StatusCode);

        var publicClient = factory.CreateClient();
        var publicResponse = await publicClient.GetAsync("/api/offerings");
        Assert.Equal(wasVisible ? HttpStatusCode.NotFound : HttpStatusCode.OK, publicResponse.StatusCode);

        // Restore state so other tests in this fixture aren't affected by ordering.
        await client.PostAsync($"/api/dashboard/nav-items/{offerings.Id}/toggle", null);
    }
}
