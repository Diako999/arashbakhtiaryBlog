using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArashBlog.Api.Tests.Auth;

// Mirrors the Django project's dashboard.tests.TwoFactorGateTests — same
// scenarios (anonymous denied, forced setup, wrong code rejected, valid
// code grants access, recovery code works once), translated to ASP.NET
// Core Identity's cookie + partial-2FA-cookie flow instead of django-otp's
// per-session is_verified() flag.
public class TwoFactorGateTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task CreateUserAsync(string username, string password)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = username, Email = $"{username}@example.com", EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    [Fact]
    public async Task Anonymous_request_to_dashboard_is_denied()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/ping");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_without_confirmed_device_requires_otp_setup()
    {
        await CreateUserAsync("no-device-user", "Sup3r-Secret!");
        var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("no-device-user", "Sup3r-Secret!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.True(login!.Succeeded);
        Assert.True(login.RequiresOtpSetup);

        var pingResponse = await client.GetAsync("/api/dashboard/ping");
        Assert.Equal(HttpStatusCode.Forbidden, pingResponse.StatusCode);
    }

    [Fact]
    public async Task Wrong_otp_code_is_rejected_during_setup()
    {
        await CreateUserAsync("wrong-code-user", "Sup3r-Secret!");
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("wrong-code-user", "Sup3r-Secret!"));

        var confirmResponse = await client.PostAsJsonAsync("/api/auth/otp/setup/confirm", new OtpCodeRequest("000000"));

        Assert.Equal(HttpStatusCode.BadRequest, confirmResponse.StatusCode);
    }

    [Fact]
    public async Task Completing_setup_with_a_valid_code_grants_dashboard_access()
    {
        await CreateUserAsync("valid-code-user", "Sup3r-Secret!");
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("valid-code-user", "Sup3r-Secret!"));

        var setupResponse = await client.GetAsync("/api/auth/otp/setup");
        var setup = await setupResponse.Content.ReadFromJsonAsync<OtpSetupResponse>();
        var validCode = GenerateTotp(setup!.ManualKey);

        var confirmResponse = await client.PostAsJsonAsync("/api/auth/otp/setup/confirm", new OtpCodeRequest(validCode));
        var confirm = await confirmResponse.Content.ReadFromJsonAsync<OtpConfirmResponse>();

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        Assert.Equal(10, confirm!.RecoveryCodes.Count);

        var pingResponse = await client.GetAsync("/api/dashboard/ping");
        Assert.Equal(HttpStatusCode.OK, pingResponse.StatusCode);
    }

    [Fact]
    public async Task Fresh_session_after_2fa_enabled_is_forced_through_verify_not_setup()
    {
        await CreateUserAsync("verify-user", "Sup3r-Secret!");
        var setupClient = factory.CreateClient();
        await setupClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("verify-user", "Sup3r-Secret!"));
        var setupResponse = await setupClient.GetAsync("/api/auth/otp/setup");
        var setup = await setupResponse.Content.ReadFromJsonAsync<OtpSetupResponse>();
        await setupClient.PostAsJsonAsync("/api/auth/otp/setup/confirm", new OtpCodeRequest(GenerateTotp(setup!.ManualKey)));

        // New session (new HttpClient == new cookie jar), same already-2FA-enabled user.
        var freshClient = factory.CreateClient();
        var loginResponse = await freshClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("verify-user", "Sup3r-Secret!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.True(login!.RequiresOtpVerify);

        var pingBeforeVerify = await freshClient.GetAsync("/api/dashboard/ping");
        Assert.Equal(HttpStatusCode.Unauthorized, pingBeforeVerify.StatusCode);

        var verifyResponse = await freshClient.PostAsJsonAsync("/api/auth/otp/verify", new OtpCodeRequest(GenerateTotp(setup.ManualKey)));
        var verify = await verifyResponse.Content.ReadFromJsonAsync<OtpVerifyResponse>();

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        Assert.False(verify!.UsedRecoveryCode);

        var pingAfterVerify = await freshClient.GetAsync("/api/dashboard/ping");
        Assert.Equal(HttpStatusCode.OK, pingAfterVerify.StatusCode);
    }

    [Fact]
    public async Task Recovery_code_grants_one_time_access()
    {
        await CreateUserAsync("recovery-user", "Sup3r-Secret!");
        var setupClient = factory.CreateClient();
        await setupClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("recovery-user", "Sup3r-Secret!"));
        var setupResponse = await setupClient.GetAsync("/api/auth/otp/setup");
        var setup = await setupResponse.Content.ReadFromJsonAsync<OtpSetupResponse>();
        var confirmResponse = await setupClient.PostAsJsonAsync(
            "/api/auth/otp/setup/confirm", new OtpCodeRequest(GenerateTotp(setup!.ManualKey)));
        var confirm = await confirmResponse.Content.ReadFromJsonAsync<OtpConfirmResponse>();
        var recoveryCode = confirm!.RecoveryCodes[0];

        var freshClient = factory.CreateClient();
        await freshClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("recovery-user", "Sup3r-Secret!"));

        var verifyResponse = await freshClient.PostAsJsonAsync("/api/auth/otp/verify", new OtpCodeRequest(recoveryCode));
        var verify = await verifyResponse.Content.ReadFromJsonAsync<OtpVerifyResponse>();

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        Assert.True(verify!.UsedRecoveryCode);

        // Same code again must fail — recovery codes are single-use.
        var anotherClient = factory.CreateClient();
        await anotherClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("recovery-user", "Sup3r-Secret!"));
        var reuseResponse = await anotherClient.PostAsJsonAsync("/api/auth/otp/verify", new OtpCodeRequest(recoveryCode));

        Assert.Equal(HttpStatusCode.BadRequest, reuseResponse.StatusCode);
    }

    // Generates a valid RFC 6238 TOTP (SHA1, 30s step, 6 digits — the same
    // parameters ASP.NET Core Identity's default authenticator provider
    // uses) for the base32 "manual key" returned by /api/auth/otp/setup.
    // No external device needed, same purpose as the totp() helper in the
    // Django project's "Local test login" doc.
    private static string GenerateTotp(string formattedKey)
    {
        var key = Base32Decode(formattedKey.Replace(" ", ""));
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var timestepBytes = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian) Array.Reverse(timestepBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(timestepBytes);
        var offset = hash[^1] & 0x0F;
        var binaryCode = ((hash[offset] & 0x7F) << 24)
                          | ((hash[offset + 1] & 0xFF) << 16)
                          | ((hash[offset + 2] & 0xFF) << 8)
                          | (hash[offset + 3] & 0xFF);
        var code = binaryCode % 1_000_000;
        return code.ToString("D6");
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        input = input.TrimEnd('=').ToUpperInvariant();

        var bits = new List<bool>();
        foreach (var c in input)
        {
            var value = alphabet.IndexOf(c);
            if (value < 0) continue;
            for (var i = 4; i >= 0; i--) bits.Add(((value >> i) & 1) == 1);
        }

        var bytes = new List<byte>();
        for (var i = 0; i + 8 <= bits.Count; i += 8)
        {
            byte b = 0;
            for (var j = 0; j < 8; j++) b = (byte)((b << 1) | (bits[i + j] ? 1 : 0));
            bytes.Add(b);
        }

        return bytes.ToArray();
    }
}
