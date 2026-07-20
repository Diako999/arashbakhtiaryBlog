using ArashBlog.Api.Common;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Data;

// Dev-only convenience seed — mirrors the Django project's
// seed_demo_content management command + its "Local test login" doc.
// Only ever invoked from Program.cs when the environment is Development.
public static class DbSeeder
{
    public const string DevAdminUsername = "admin";
    public const string DevAdminPassword = "Dev-Only-Not-For-Prod-1!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        if (!await db.NavItems.AnyAsync(n => n.Key == "blog"))
        {
            db.NavItems.Add(new NavItem
            {
                Key = "blog",
                Path = "/blog",
                TitleFa = "وبلاگ",
                TitleCkb = "بلۆگ",
                IsVisible = true,
                Order = 1,
            });
            await db.SaveChangesAsync();
        }

        var admin = await userManager.FindByNameAsync(DevAdminUsername);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DevAdminUsername,
                Email = "admin@example.com",
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(admin, DevAdminPassword);
        }

        if (!await db.Categories.AnyAsync())
        {
            var category = new Category
            {
                NameFa = "یادداشت‌ها",
                NameCkb = "تێبینییەکان",
                Slug = Slugifier.Slugify("یادداشت‌ها"),
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var post = new Post
            {
                TitleFa = "نخستین نوشته وبلاگ",
                TitleCkb = "یەکەم بابەتی بلۆگ",
                Slug = Slugifier.Slugify("نخستین نوشته وبلاگ"),
                ExcerptFa = "یک نوشته نمونه برای تست نمایش لیست و جزئیات.",
                ExcerptCkb = "بابەتێکی نموونەیی بۆ تاقیکردنەوەی پیشاندانی لیست و وردەکاری.",
                BodyFa = PostBodySanitizer.Sanitize("<p>این یک نوشته نمونه برای milestone یک است.</p>"),
                BodyCkb = PostBodySanitizer.Sanitize("<p>ئەمە بابەتێکی نموونەیە بۆ milestone یەک.</p>"),
                Status = PostStatus.Published,
                PublishedAt = DateTimeOffset.UtcNow,
                AuthorId = admin.Id,
                CategoryId = category.Id,
            };
            db.Posts.Add(post);
            await db.SaveChangesAsync();
        }
    }
}
