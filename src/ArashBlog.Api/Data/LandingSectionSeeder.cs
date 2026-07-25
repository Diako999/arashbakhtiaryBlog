using ArashBlog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Data;

// Idempotent, insert-missing-rows-only — same pattern as NavItemSeeder.
// Seeds the fixed 5 LandingSection rows (see LandingSection.cs) with
// generic, honest placeholder copy only. Deliberately does NOT invent
// fabricated stats/numbers (a "+8 years experience" / "500+ people"
// trust-row style badge) the way the original design mockup did — that
// exact mistake was already made and corrected once on the Django
// version of this same site; the real fix is admin-edited real copy,
// not a differently-fake placeholder.
public static class LandingSectionSeeder
{
    private static readonly (LandingSectionType Type, int Order, string HeadingFa, string HeadingCkb)[] Expected =
    [
        (LandingSectionType.Hero, 0, "خوش آمدید", "بەخێربێیت"),
        (LandingSectionType.OfferingsTeaser, 1, "دوره‌ها", "کۆرسەکان"),
        (LandingSectionType.PostsTeaser, 2, "آخرین یادداشت‌ها", "دوایین بابەتەکان"),
        (LandingSectionType.TestimonialsTeaser, 3, "نظرات", "ڕاوبۆچوونەکان"),
        (LandingSectionType.CtaBanner, 4, "با ما در ارتباط باشید", "پەیوەندیمان پێوە بکە"),
    ];

    public static async Task EnsureAsync(ApplicationDbContext db)
    {
        var existingTypes = await db.LandingSections.Select(s => s.Type).ToListAsync();

        foreach (var item in Expected)
        {
            if (existingTypes.Contains(item.Type)) continue;

            db.LandingSections.Add(new LandingSection
            {
                Type = item.Type,
                Order = item.Order,
                IsVisible = true,
                HeadingFa = item.HeadingFa,
                HeadingCkb = item.HeadingCkb,
            });
        }

        await db.SaveChangesAsync();
    }
}
