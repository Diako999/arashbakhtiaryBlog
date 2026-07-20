using System.Linq;
using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using ArashBlog.Api.Features.Pages;
using Xunit;

namespace ArashBlog.Api.Tests.Pages;

public class PagesTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Detail_404s_while_the_pages_section_is_hidden()
    {
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "pages", false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/pages/about");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task About_and_contact_are_seeded_and_readable_once_visible()
    {
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "pages", true);
        var client = factory.CreateClient();

        var about = await client.GetFromJsonAsync<FlatPageDto>("/api/pages/about?lang=fa");
        var contact = await client.GetFromJsonAsync<FlatPageDto>("/api/pages/contact?lang=fa");

        Assert.Equal("about", about!.Slug);
        Assert.NotEmpty(about.Title);
        Assert.Equal("contact", contact!.Slug);
        Assert.NotEmpty(contact.Title);
    }

    [Fact]
    public async Task Unknown_slug_404s_even_when_visible()
    {
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "pages", true);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/pages/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_can_edit_flat_page_content_and_it_reflects_publicly()
    {
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "pages", true);
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "flatpage-admin-1");

        var list = await admin.GetFromJsonAsync<List<DashboardFlatPageDto>>("/api/dashboard/flat-pages");
        var about = list!.Single(p => p.Slug == "about");

        var updateResponse = await admin.PutAsJsonAsync($"/api/dashboard/flat-pages/{about.Id}", new UpsertFlatPageRequest(
            "درباره ما - نسخه ویرایش‌شده", about.TitleCkb, "<p>محتوای جدید</p>", about.BodyCkb, "", "", "", ""));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var publicClient = factory.CreateClient();
        var publicAbout = await publicClient.GetFromJsonAsync<FlatPageDto>("/api/pages/about?lang=fa");
        Assert.Equal("درباره ما - نسخه ویرایش‌شده", publicAbout!.Title);
        Assert.Contains("محتوای جدید", publicAbout.BodyHtml);
    }
}
