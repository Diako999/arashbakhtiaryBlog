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
            s.InstagramUrl, s.TelegramUrl, s.TwitterUrl, s.LinkedinUrl, s.WhatsappUrl, s.MetaDescription));
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
        s.MetaDescription = request.MetaDescription;
        await db.SaveChangesAsync();

        return Ok(new SiteSettingDto(
            s.SiteName, s.LogoUrl, s.ContactEmail, s.ContactPhone,
            s.InstagramUrl, s.TelegramUrl, s.TwitterUrl, s.LinkedinUrl, s.WhatsappUrl, s.MetaDescription));
    }

    [HttpGet("theme")]
    public async Task<ActionResult<ThemeDto>> GetTheme()
    {
        var t = await SingletonLoader.LoadThemeConfigAsync(db);
        return Ok(ToThemeDto(t));
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

        if (!Enum.TryParse<ThemeFont>(request.FontChoice, ignoreCase: true, out var font))
        {
            return BadRequest(new { error = "invalid_font" });
        }

        if (!Enum.TryParse<ThemeCardStyle>(request.CardStyle, ignoreCase: true, out var cardStyle))
        {
            return BadRequest(new { error = "invalid_card_style" });
        }

        if (!Enum.TryParse<ThemeHeaderFooterStyle>(request.HeaderFooterStyle, ignoreCase: true, out var headerFooterStyle))
        {
            return BadRequest(new { error = "invalid_header_footer_style" });
        }

        var t = await SingletonLoader.LoadThemeConfigAsync(db);
        t.BrandColor = request.BrandColor;
        t.AccentColor = request.AccentColor;
        t.DefaultMode = mode;
        t.FontChoice = font;
        t.CardStyle = cardStyle;
        t.HeaderFooterStyle = headerFooterStyle;
        await db.SaveChangesAsync();

        return Ok(ToThemeDto(t));
    }

    private static ThemeDto ToThemeDto(ThemeConfig t) => new(
        t.BrandColor, t.AccentColor, t.DefaultMode.ToString(),
        t.FontChoice.ToString(), t.CardStyle.ToString(), t.HeaderFooterStyle.ToString());
}
