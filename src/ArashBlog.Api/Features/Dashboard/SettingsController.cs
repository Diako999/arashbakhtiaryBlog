using System.Text.RegularExpressions;
using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Site;
using Microsoft.AspNetCore.Mvc;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's SettingsView + SiteSettingForm/
// ThemeConfigForm. Both settings are singleton rows loaded via
// SingletonLoader, same "get or create with defaults" as the public
// SiteController reads from.
[ApiController]
[Route("api/dashboard/settings")]
[RequireVerifiedTwoFactor]
public partial class SettingsController(ApplicationDbContext db) : ControllerBase
{
    [GeneratedRegex(@"^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();

    [HttpGet("site")]
    public async Task<ActionResult<SiteSettingDto>> GetSite()
    {
        var s = await SingletonLoader.LoadSiteSettingAsync(db);
        return Ok(new SiteSettingDto(
            s.SiteName, s.LogoUrl, s.ContactEmail, s.ContactPhone,
            s.InstagramUrl, s.TelegramUrl, s.TwitterUrl, s.LinkedinUrl, s.WhatsappUrl, s.YoutubeUrl, s.MetaDescription));
    }

    [HttpPut("site")]
    public async Task<ActionResult<SiteSettingDto>> UpdateSite(UpsertSiteSettingRequest request)
    {
        var s = await SingletonLoader.LoadSiteSettingAsync(db);
        s.SiteName = request.SiteName;
        s.LogoUrl = request.LogoUrl;
        s.ContactEmail = request.ContactEmail;
        s.ContactPhone = request.ContactPhone;
        s.InstagramUrl = request.InstagramUrl;
        s.TelegramUrl = request.TelegramUrl;
        s.TwitterUrl = request.TwitterUrl;
        s.LinkedinUrl = request.LinkedinUrl;
        s.WhatsappUrl = request.WhatsappUrl;
        s.YoutubeUrl = request.YoutubeUrl;
        s.MetaDescription = request.MetaDescription;
        await db.SaveChangesAsync();

        return Ok(new SiteSettingDto(
            s.SiteName, s.LogoUrl, s.ContactEmail, s.ContactPhone,
            s.InstagramUrl, s.TelegramUrl, s.TwitterUrl, s.LinkedinUrl, s.WhatsappUrl, s.YoutubeUrl, s.MetaDescription));
    }

    [HttpGet("theme")]
    public async Task<ActionResult<ThemeDto>> GetTheme()
    {
        var t = await SingletonLoader.LoadThemeConfigAsync(db);
        return Ok(new ThemeDto(t.BrandColor, t.AccentColor, t.DefaultMode.ToString()));
    }

    [HttpPut("theme")]
    public async Task<ActionResult<ThemeDto>> UpdateTheme(UpsertThemeRequest request)
    {
        if (!HexColorRegex().IsMatch(request.BrandColor) || !HexColorRegex().IsMatch(request.AccentColor))
        {
            return BadRequest(new { error = "invalid_color" });
        }

        if (!Enum.TryParse<ThemeMode>(request.DefaultMode, ignoreCase: true, out var mode))
        {
            return BadRequest(new { error = "invalid_mode" });
        }

        var t = await SingletonLoader.LoadThemeConfigAsync(db);
        t.BrandColor = request.BrandColor;
        t.AccentColor = request.AccentColor;
        t.DefaultMode = mode;
        await db.SaveChangesAsync();

        return Ok(new ThemeDto(t.BrandColor, t.AccentColor, t.DefaultMode.ToString()));
    }

    [HttpGet("landing")]
    public async Task<ActionResult<DashboardLandingPageSettingsDto>> GetLanding()
    {
        var l = await SingletonLoader.LoadLandingPageSettingsAsync(db);
        return Ok(ToDto(l));
    }

    [HttpPut("landing")]
    public async Task<ActionResult<DashboardLandingPageSettingsDto>> UpdateLanding(DashboardLandingPageSettingsDto request)
    {
        var l = await SingletonLoader.LoadLandingPageSettingsAsync(db);
        l.HeroBadgeFa = request.HeroBadgeFa;
        l.HeroBadgeCkb = request.HeroBadgeCkb;
        l.HeroSubtitleFa = request.HeroSubtitleFa;
        l.HeroSubtitleCkb = request.HeroSubtitleCkb;
        l.HeroDescriptionFa = request.HeroDescriptionFa;
        l.HeroDescriptionCkb = request.HeroDescriptionCkb;
        l.AboutRoleFa = request.AboutRoleFa;
        l.AboutRoleCkb = request.AboutRoleCkb;
        l.AboutBioFa = request.AboutBioFa;
        l.AboutBioCkb = request.AboutBioCkb;
        l.AboutPhotoUrl = request.AboutPhotoUrl;
        l.AboutGithubUrl = request.AboutGithubUrl;
        l.AboutYoutubeUrl = request.AboutYoutubeUrl;
        await db.SaveChangesAsync();

        return Ok(ToDto(l));
    }

    private static DashboardLandingPageSettingsDto ToDto(Domain.LandingPageSettings l) => new(
        l.HeroBadgeFa, l.HeroBadgeCkb, l.HeroSubtitleFa, l.HeroSubtitleCkb,
        l.HeroDescriptionFa, l.HeroDescriptionCkb, l.AboutRoleFa, l.AboutRoleCkb,
        l.AboutBioFa, l.AboutBioCkb, l.AboutPhotoUrl, l.AboutGithubUrl, l.AboutYoutubeUrl);
}
