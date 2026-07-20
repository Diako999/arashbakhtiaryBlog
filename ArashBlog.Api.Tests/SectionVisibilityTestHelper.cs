using ArashBlog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArashBlog.Api.Tests;

// NavItemSeeder ensures "offerings"/"testimonials"/"leads" rows exist
// (hidden by default) on every app startup, including in tests. This just
// flips IsVisible directly so public-endpoint tests don't need to go
// through the dashboard toggle endpoint every time.
public static class SectionVisibilityTestHelper
{
    public static async Task SetVisibleAsync(TestWebApplicationFactory factory, string key, bool visible)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = await db.NavItems.SingleAsync(n => n.Key == key);
        item.IsVisible = visible;
        await db.SaveChangesAsync();
    }
}
