using ArashBlog.Api.Common;
using ArashBlog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Data;

// Unconditional, like NavItemSeeder — the /about and /contact routes need
// *something* to render even before an admin edits them, and unlike
// Django (which left FlatPage content editing to the stock admin as a
// fallback), this rewrite has no admin surface at all, so these two rows
// must exist proactively rather than be created on demand.
public static class FlatPageSeeder
{
    public static async Task EnsureAsync(ApplicationDbContext db)
    {
        if (!await db.FlatPages.AnyAsync(p => p.Slug == "about"))
        {
            db.FlatPages.Add(new FlatPage
            {
                Slug = "about",
                TitleFa = "درباره ما",
                TitleCkb = "دەربارەی ئێمە",
                BodyFa = PostBodySanitizer.Sanitize("<p>این صفحه هنوز ویرایش نشده است.</p>"),
                BodyCkb = PostBodySanitizer.Sanitize("<p>ئەم پەڕەیە هێشتا دەستکاری نەکراوە.</p>"),
            });
        }

        if (!await db.FlatPages.AnyAsync(p => p.Slug == "contact"))
        {
            db.FlatPages.Add(new FlatPage
            {
                Slug = "contact",
                TitleFa = "تماس با ما",
                TitleCkb = "پەیوەندیمان پێوە بکە",
                BodyFa = PostBodySanitizer.Sanitize("<p>این صفحه هنوز ویرایش نشده است.</p>"),
                BodyCkb = PostBodySanitizer.Sanitize("<p>ئەم پەڕەیە هێشتا دەستکاری نەکراوە.</p>"),
            });
        }

        await db.SaveChangesAsync();
    }
}
