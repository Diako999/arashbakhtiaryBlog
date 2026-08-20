using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            s.InstagramUrl, s.TelegramUrl, s.TwitterUrl, s.LinkedinUrl, s.WhatsappUrl, s.YoutubeUrl, s.MetaDescription));
    }

    [HttpGet("hero-slides")]
    public async Task<ActionResult<IReadOnlyList<HeroSlideDto>>> HeroSlides()
    {
        var slides = await db.HeroSlides
            .OrderBy(h => h.Order)
            .ThenBy(h => h.CreatedAt)
            .Select(h => new HeroSlideDto(h.ImageUrl, h.LinkUrl))
            .ToListAsync();

        return Ok(slides);
    }

    [HttpGet("theme")]
    public async Task<ActionResult<ThemeDto>> Theme()
    {
        var t = await SingletonLoader.LoadThemeConfigAsync(db);
        return Ok(new ThemeDto(t.BrandColor, t.AccentColor, t.DefaultMode.ToString()));
    }

    [HttpGet("landing")]
    public async Task<ActionResult<LandingPageSettingsDto>> Landing([FromQuery] string? lang)
    {
        var isCkb = lang == "ckb";
        var l = await SingletonLoader.LoadLandingPageSettingsAsync(db);
        return Ok(new LandingPageSettingsDto(
            isCkb ? l.HeroBadgeCkb : l.HeroBadgeFa,
            isCkb ? l.HeroSubtitleCkb : l.HeroSubtitleFa,
            isCkb ? l.HeroDescriptionCkb : l.HeroDescriptionFa,
            isCkb ? l.AboutRoleCkb : l.AboutRoleFa,
            isCkb ? l.AboutBioCkb : l.AboutBioFa,
            l.AboutPhotoUrl,
            l.AboutGithubUrl,
            l.AboutYoutubeUrl));
    }
}
