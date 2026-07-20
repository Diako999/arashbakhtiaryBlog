using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace ArashBlog.Api.Data;

// Production has no equivalent to Django's `createsuperuser` CLI command —
// DbSeeder's demo admin only runs in Development, and there's no user
// registration endpoint (deliberately: this app has exactly one admin,
// created up front, not a self-service signup flow). Without this, a
// fresh production deploy would have no way to ever log in.
//
// Safe to leave wired in permanently: it's a no-op the instant any user
// exists, and a no-op if the InitialAdmin config isn't set — so nothing
// bad happens if this runs on every future startup after the real admin
// account already exists.
public static class AdminBootstrapper
{
    public static async Task EnsureAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (userManager.Users.Any()) return;

        var username = configuration["InitialAdmin:Username"];
        var password = configuration["InitialAdmin:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return;

        var admin = new ApplicationUser { UserName = username, Email = $"{username}@localhost", EmailConfirmed = true };
        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"InitialAdmin bootstrap failed: {errors}");
        }
    }
}
