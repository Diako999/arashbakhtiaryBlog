using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace ArashBlog.Api.Features.Site;

// Not gated by RequireVisibleSection — site branding and theme colors are
// always needed for the public layout (header/footer/CSS vars), same as
// the Django project's site_settings/theme context processors running on
// every request regardless of which sections are currently visible.
[ApiController]
[Route("api/site")]
public class SiteController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<ActionResult<SiteSettingDto>> Settings()
    {
        var s = await SingletonLoader.LoadSiteSettingAsync(db);
        return Ok(new SiteSettingDto(
            s.SiteName, s.LogoUrl, s.ContactEmail, s.ContactPhone,
            s.InstagramUrl, s.TelegramUrl, s.TwitterUrl, s.LinkedinUrl, s.WhatsappUrl, s.MetaDescription));
    }

    [HttpGet("theme")]
    public async Task<ActionResult<ThemeDto>> Theme()
    {
        var t = await SingletonLoader.LoadThemeConfigAsync(db);
        return Ok(new ThemeDto(
            t.BrandColor, t.AccentColor, t.DefaultMode.ToString(),
            t.FontChoice.ToString(), t.CardStyle.ToString(), t.HeaderFooterStyle.ToString()));
    }
}
