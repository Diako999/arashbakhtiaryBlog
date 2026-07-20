using ArashBlog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Data;

// Idempotent, environment-independent — unlike DbSeeder (dev-only demo
// admin/content), these NavItem rows are real application configuration
// the phased-rollout mechanism depends on in every environment, matching
// how the Django project seeded them via a navigation migration rather
// than a dev-only management command. Runs on every startup; a missing
// row gets created once, an existing row's IsVisible/order is left alone
// so an admin's toggle from the dashboard is never silently reverted.
public static class NavItemSeeder
{
    private static readonly (string Key, string Path, string TitleFa, string TitleCkb, bool DefaultVisible, int Order)[] Expected =
    [
        ("blog", "/blog", "وبلاگ", "بلۆگ", true, 1),
        ("offerings", "/offerings", "دوره‌ها", "کۆرسەکان", false, 2),
        ("testimonials", "/testimonials", "نظرات", "ڕاوبۆچوونەکان", false, 3),
        ("leads", "/free-resource", "منابع رایگان", "سەرچاوەی بەخۆڕایی", false, 4),
    ];

    public static async Task EnsureAsync(ApplicationDbContext db)
    {
        var existingKeys = await db.NavItems.Select(n => n.Key).ToListAsync();

        foreach (var item in Expected)
        {
            if (existingKeys.Contains(item.Key)) continue;

            db.NavItems.Add(new NavItem
            {
                Key = item.Key,
                Path = item.Path,
                TitleFa = item.TitleFa,
                TitleCkb = item.TitleCkb,
                IsVisible = item.DefaultVisible,
                Order = item.Order,
            });
        }

        await db.SaveChangesAsync();
    }
}
