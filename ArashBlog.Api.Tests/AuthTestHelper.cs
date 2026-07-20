using System.Linq;
using System.Net.Http.Json;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArashBlog.Api.Tests;

// Shared "give me a fully 2FA-verified dashboard client" helper for tests
// that exercise [RequireVerifiedTwoFactor] endpoints but aren't themselves
// testing the 2FA flow (that's TwoFactorGateTests' job).
public static class AuthTestHelper
{
    public const string Password = "Sup3r-Secret!";

    public static async Task<HttpClient> CreateVerifiedClientAsync(TestWebApplicationFactory factory, string username)
    {
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = username, Email = $"{username}@example.com", EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, Password);
            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, Password));

        var setupResponse = await client.GetAsync("/api/auth/otp/setup");
        var setup = await setupResponse.Content.ReadFromJsonAsync<OtpSetupResponse>();
        var code = TotpHelper.Generate(setup!.ManualKey);
        await client.PostAsJsonAsync("/api/auth/otp/setup/confirm", new OtpCodeRequest(code));

        return client;
    }
}
