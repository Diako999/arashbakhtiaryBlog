using ArashBlog.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ArashBlog.Api.Features.Auth;

// Placeholder — proves RequireVerifiedTwoFactorAttribute gates correctly
// end to end. M2 replaces/extends this with the real Dashboard content
// endpoints (Overview, Content CRUD, etc.), reusing the same attribute.
[ApiController]
[Route("api/dashboard")]
[RequireVerifiedTwoFactor]
public class DashboardPingController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true });
}
