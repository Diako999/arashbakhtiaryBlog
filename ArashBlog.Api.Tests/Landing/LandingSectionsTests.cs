using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using ArashBlog.Api.Features.Landing;
using Xunit;

namespace ArashBlog.Api.Tests.Landing;

public class LandingSectionsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Toggle_hides_a_section_from_the_public_response()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "landing-admin-1");
        var sections = await admin.GetFromJsonAsync<List<DashboardLandingSectionDto>>("/api/dashboard/landing-sections");
        var hero = sections!.Single(s => s.Type == "Hero");

        var client = factory.CreateClient();
        var before = await client.GetFromJsonAsync<List<LandingSectionDto>>("/api/landing?lang=fa");
        Assert.Contains(before!, s => s.Type == "Hero");

        await admin.PostAsync($"/api/dashboard/landing-sections/{hero.Id}/toggle", null);

        var after = await client.GetFromJsonAsync<List<LandingSectionDto>>("/api/landing?lang=fa");
        Assert.DoesNotContain(after!, s => s.Type == "Hero");
    }

    [Fact]
    public async Task Move_up_renumbers_order()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "landing-admin-2");
        var before = await admin.GetFromJsonAsync<List<DashboardLandingSectionDto>>("/api/dashboard/landing-sections");
        var postsTeaser = before!.Single(s => s.Type == "PostsTeaser");
        var indexBefore = before!.FindIndex(s => s.Id == postsTeaser.Id);
        Assert.True(indexBefore > 0);

        await admin.PostAsync($"/api/dashboard/landing-sections/{postsTeaser.Id}/move/up", null);

        var after = await admin.GetFromJsonAsync<List<DashboardLandingSectionDto>>("/api/dashboard/landing-sections");
        var indexAfter = after!.FindIndex(s => s.Id == postsTeaser.Id);
        Assert.True(indexAfter < indexBefore);
    }

    [Fact]
    public async Task OfferingsTeaser_is_hidden_while_the_offerings_NavItem_is_hidden()
    {
        // NavItemSeeder seeds "offerings" hidden by default, so the section
        // (IsVisible=true from LandingSectionSeeder) should still be absent
        // from the public response until an admin makes the section visible.
        var client = factory.CreateClient();
        var beforeVisible = await client.GetFromJsonAsync<List<LandingSectionDto>>("/api/landing?lang=fa");
        Assert.DoesNotContain(beforeVisible!, s => s.Type == "OfferingsTeaser");

        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "offerings", true);

        var afterVisible = await client.GetFromJsonAsync<List<LandingSectionDto>>("/api/landing?lang=fa");
        Assert.Contains(afterVisible!, s => s.Type == "OfferingsTeaser");
    }
}
