namespace ArashBlog.Api.Features.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(bool Succeeded, bool RequiresOtpSetup, bool RequiresOtpVerify);

public record MeResponse(bool IsAuthenticated, bool RequiresOtpSetup, bool RequiresOtpVerify, string? Username);

public record OtpSetupResponse(string QrCodeDataUri, string ManualKey);

public record OtpCodeRequest(string Code);

public record OtpConfirmResponse(IReadOnlyList<string> RecoveryCodes);

public record OtpVerifyResponse(bool UsedRecoveryCode);
