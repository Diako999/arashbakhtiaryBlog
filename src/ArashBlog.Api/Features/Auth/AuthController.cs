using System.Text;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QRCoder;

namespace ArashBlog.Api.Features.Auth;

// Mirrors apps/dashboard/otp_views.py's flow: password sign-in, then a
// forced TOTP setup (first time) or verify (every later session) gate
// before dashboard-tier access, plus one-time-display recovery codes.
[ApiController]
[Route("api/auth")]
public class AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    : ControllerBase
{
    private const string Issuer = "ArashBlog";

    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null) return Ok(new MeResponse(false, false, false, null));
            return Ok(new MeResponse(true, !user.TwoFactorEnabled, false, user.UserName));
        }

        // A password-verified-but-not-yet-2FA-verified session lives on
        // Identity's own partial "two factor" cookie, separate from the
        // full Identity.Application cookie — User.Identity.IsAuthenticated
        // is false here, so this is how the SPA learns "route to /otp/verify"
        // rather than treating the visitor as fully anonymous.
        var pending = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (pending is not null)
        {
            return Ok(new MeResponse(false, false, true, pending.UserName));
        }

        return Ok(new MeResponse(false, false, false, null));
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Username, request.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            return Ok(new LoginResponse(true, false, true));
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new LoginResponse(false, false, false));
        }

        var user = await userManager.FindByNameAsync(request.Username);
        var requiresSetup = user is not null && !user.TwoFactorEnabled;
        return Ok(new LoginResponse(true, requiresSetup, false));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("otp/setup")]
    [Authorize]
    public async Task<ActionResult<OtpSetupResponse>> OtpSetup()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (user.TwoFactorEnabled)
        {
            return Conflict(new { reason = "already_configured" });
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = user.Email ?? user.UserName!;
        var otpauthUri = $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(email)}" +
                          $"?secret={key}&issuer={Uri.EscapeDataString(Issuer)}&digits=6";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(otpauthUri, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrData);
        var qrBytes = pngQrCode.GetGraphic(10);
        var dataUri = "data:image/png;base64," + Convert.ToBase64String(qrBytes);

        return Ok(new OtpSetupResponse(dataUri, FormatKey(key!)));
    }

    [HttpPost("otp/setup/confirm")]
    [Authorize]
    [EnableRateLimiting("otp")]
    public async Task<ActionResult<OtpConfirmResponse>> OtpSetupConfirm(OtpCodeRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);
        if (!isValid)
        {
            return BadRequest(new { error = "invalid_code" });
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return Ok(new OtpConfirmResponse(codes!.ToList()));
    }

    [HttpPost("otp/recovery-codes/regenerate")]
    [Authorize]
    public async Task<ActionResult<OtpConfirmResponse>> RegenerateRecoveryCodes()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.TwoFactorEnabled) return Unauthorized();

        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return Ok(new OtpConfirmResponse(codes!.ToList()));
    }

    [HttpPost("otp/verify")]
    [EnableRateLimiting("otp")]
    public async Task<ActionResult<OtpVerifyResponse>> OtpVerify(OtpCodeRequest request)
    {
        var pending = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (pending is null) return Unauthorized();

        var code = request.Code.Replace(" ", "").Replace("-", "");
        var totpResult = await signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false);
        if (totpResult.Succeeded)
        {
            return Ok(new OtpVerifyResponse(false));
        }

        // Recovery-code fallback, same order as the Django OTPVerifyView:
        // try a TOTP code first, only then try it as a one-time backup code.
        var recoveryResult = await signInManager.TwoFactorRecoveryCodeSignInAsync(request.Code);
        if (recoveryResult.Succeeded)
        {
            return Ok(new OtpVerifyResponse(true));
        }

        return BadRequest(new { error = "invalid_code" });
    }

    private static string FormatKey(string unformattedKey)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < unformattedKey.Length; i += 4)
        {
            sb.Append(unformattedKey.AsSpan(i, Math.Min(4, unformattedKey.Length - i)));
            sb.Append(' ');
        }
        return sb.ToString().Trim();
    }
}
