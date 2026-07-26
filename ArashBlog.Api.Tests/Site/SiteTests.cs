using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using ArashBlog.Api.Features.Site;
using Xunit;

namespace ArashBlog.Api.Tests.Site;

public class SiteTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Settings_and_theme_are_always_available_without_any_section_being_visible()
    {
        var client = factory.CreateClient();

        var settingsResponse = await client.GetAsync("/api/site/settings");
        var themeResponse = await client.GetAsync("/api/site/theme");

        Assert.Equal(HttpStatusCode.OK, settingsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, themeResponse.StatusCode);
    }

    [Fact]
    public async Task Dashboard_can_update_theme_and_it_reflects_on_the_public_endpoint()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "theme-admin-1");

        var updateResponse = await admin.PutAsJsonAsync("/api/dashboard/settings/theme", new UpsertThemeRequest("#123456", "#abcdef", "Light", "Vazirmatn", "Rounded", "Glass"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var publicClient = factory.CreateClient();
        var theme = await publicClient.GetFromJsonAsync<ThemeDto>("/api/site/theme");

        Assert.Equal("#123456", theme!.BrandColor);
        Assert.Equal("#abcdef", theme.AccentColor);
        Assert.Equal("Light", theme.DefaultMode);
    }

    [Fact]
    public async Task Theme_update_rejects_an_invalid_hex_color()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "theme-admin-2");

        var response = await admin.PutAsJsonAsync("/api/dashboard/settings/theme", new UpsertThemeRequest("not-a-color", "#abcdef", "Dark", "Vazirmatn", "Rounded", "Glass"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Theme_update_rejects_an_invalid_extended_control()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "theme-admin-3");

        var response = await admin.PutAsJsonAsync(
            "/api/dashboard/settings/theme", new UpsertThemeRequest("#123456", "#abcdef", "Dark", "NotAFont", "Rounded", "Glass"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Theme_update_persists_extended_controls()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "theme-admin-4");

        var updateResponse = await admin.PutAsJsonAsync(
            "/api/dashboard/settings/theme", new UpsertThemeRequest("#123456", "#abcdef", "Dark", "Sahel", "Sharp", "Solid"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var publicClient = factory.CreateClient();
        var theme = await publicClient.GetFromJsonAsync<ThemeDto>("/api/site/theme");

        Assert.Equal("Sahel", theme!.FontChoice);
        Assert.Equal("Sharp", theme.CardStyle);
        Assert.Equal("Solid", theme.HeaderFooterStyle);
    }

    [Fact]
    public async Task Dashboard_can_update_site_settings_and_it_reflects_publicly()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "site-admin-1");

        var request = new UpsertSiteSettingRequest(
            "وبلاگ آرش", null, "contact@example.com", "+98 912 000 0000",
            "https://instagram.com/arash", "", "", "", "", "توضیح سایت");
        var updateResponse = await admin.PutAsJsonAsync("/api/dashboard/settings/site", request);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var publicClient = factory.CreateClient();
        var settings = await publicClient.GetFromJsonAsync<SiteSettingDto>("/api/site/settings");

        Assert.Equal("وبلاگ آرش", settings!.SiteName);
        Assert.Equal("https://instagram.com/arash", settings.InstagramUrl);
    }
}
