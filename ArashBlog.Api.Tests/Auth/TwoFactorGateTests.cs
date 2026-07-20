using System.Linq;
using System.Net;
using System.Net.Http.Json;
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

        var response = await client.GetAsync("/api/dashboard/overview");

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

        var pingResponse = await client.GetAsync("/api/dashboard/overview");
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
        var validCode = TotpHelper.Generate(setup!.ManualKey);

        var confirmResponse = await client.PostAsJsonAsync("/api/auth/otp/setup/confirm", new OtpCodeRequest(validCode));
        var confirm = await confirmResponse.Content.ReadFromJsonAsync<OtpConfirmResponse>();

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        Assert.Equal(10, confirm!.RecoveryCodes.Count);

        var pingResponse = await client.GetAsync("/api/dashboard/overview");
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
        await setupClient.PostAsJsonAsync("/api/auth/otp/setup/confirm", new OtpCodeRequest(TotpHelper.Generate(setup!.ManualKey)));

        // New session (new HttpClient == new cookie jar), same already-2FA-enabled user.
        var freshClient = factory.CreateClient();
        var loginResponse = await freshClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("verify-user", "Sup3r-Secret!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.True(login!.RequiresOtpVerify);

        var pingBeforeVerify = await freshClient.GetAsync("/api/dashboard/overview");
        Assert.Equal(HttpStatusCode.Unauthorized, pingBeforeVerify.StatusCode);

        var verifyResponse = await freshClient.PostAsJsonAsync("/api/auth/otp/verify", new OtpCodeRequest(TotpHelper.Generate(setup.ManualKey)));
        var verify = await verifyResponse.Content.ReadFromJsonAsync<OtpVerifyResponse>();

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        Assert.False(verify!.UsedRecoveryCode);

        var pingAfterVerify = await freshClient.GetAsync("/api/dashboard/overview");
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
            "/api/auth/otp/setup/confirm", new OtpCodeRequest(TotpHelper.Generate(setup!.ManualKey)));
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

}
