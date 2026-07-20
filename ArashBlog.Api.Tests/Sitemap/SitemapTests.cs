using System.Net.Http.Json;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Sitemap;

public class SitemapTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Includes_published_posts_but_excludes_a_hidden_offerings_section()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "sitemap-admin-1");
        var postResponse = await admin.PostAsJsonAsync("/api/dashboard/posts", new UpsertPostRequest(
            "sitemap-post", null, "", null, "Published", DateTimeOffset.UtcNow,
            "پست سایت‌مپ", "بابەتی سایتمەپ", "", "", "<p>x</p>", "<p>x</p>", "", "", "", ""));
        await postResponse.Content.ReadFromJsonAsync<DashboardPostDetailDto>();

        await admin.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "sitemap-offering", null, null, "Published", "دوره سایت‌مپ", "کۆرسی سایتمەپ", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "", []));
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "offerings", false);

        var client = factory.CreateClient();
        var xml = await client.GetStringAsync("/sitemap.xml");

        Assert.Contains("sitemap-post", xml);
        Assert.DoesNotContain("sitemap-offering", xml);
    }

    [Fact]
    public async Task Includes_offering_urls_once_the_section_is_visible()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "sitemap-admin-2");
        await admin.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "visible-sitemap-offering", null, null, "Published", "دوره دیدنی", "کۆرسی دیار", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "", []));
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "offerings", true);

        var client = factory.CreateClient();
        var xml = await client.GetStringAsync("/sitemap.xml");

        Assert.Contains("visible-sitemap-offering", xml);
    }

    [Fact]
    public async Task Response_is_well_formed_xml_with_the_sitemap_namespace()
    {
        var client = factory.CreateClient();

        var xml = await client.GetStringAsync("/sitemap.xml");

        Assert.Contains("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", xml);
        Assert.Contains("http://www.sitemaps.org/schemas/sitemap/0.9", xml);
    }
}
