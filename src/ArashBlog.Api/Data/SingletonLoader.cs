using ArashBlog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Data;

// Get-or-create for the two singleton config rows — mirrors the Django
// project's SingletonModel.load() classmethod. One shared code path
// instead of a startup seeder duplicating the same defaults. Deliberately
// doesn't filter/assign by a hardcoded Id: both Id columns are database
// IDENTITY, so forcing Id=1 client-side would just be ignored by EF
// (SQL Server assigns the real value) — since each table only ever holds
// this one row, taking "the first (only) row" is simpler and correct
// regardless of what identity value it actually got.
public static class SingletonLoader
{
    public static async Task<SiteSetting> LoadSiteSettingAsync(ApplicationDbContext db)
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync();
        if (setting is null)
        {
            setting = new SiteSetting();
            db.SiteSettings.Add(setting);
            await db.SaveChangesAsync();
        }
        return setting;
    }

    public static async Task<ThemeConfig> LoadThemeConfigAsync(ApplicationDbContext db)
    {
        var theme = await db.ThemeConfigs.FirstOrDefaultAsync();
        if (theme is null)
        {
            theme = new ThemeConfig();
            db.ThemeConfigs.Add(theme);
            await db.SaveChangesAsync();
        }
        return theme;
    }

    public static async Task<LandingPageSettings> LoadLandingPageSettingsAsync(ApplicationDbContext db)
    {
        var landing = await db.LandingPageSettings.FirstOrDefaultAsync();
        if (landing is null)
        {
            landing = new LandingPageSettings();
            db.LandingPageSettings.Add(landing);
            await db.SaveChangesAsync();
        }
        return landing;
    }
}
