using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArashBlog.Api.Common;

// The 2FA half of dashboard access — .NET equivalent of the Django
// project's OTPRequiredMixin / dashboard_login_required. Plain [Authorize]
// only checks that the request is authenticated; a user who's signed in
// but hasn't finished 2FA setup (TwoFactorEnabled == false) is still
// turned away here, the same way Django forces them to /otp/setup first
// instead of treating a bare login as enough for dashboard access.
public class RequireVerifiedTwoFactorAttribute() : TypeFilterAttribute(typeof(RequireVerifiedTwoFactorFilter));

public class RequireVerifiedTwoFactorFilter(UserManager<ApplicationUser> userManager) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpUser = context.HttpContext.User;
        if (httpUser.Identity?.IsAuthenticated != true)
        {
            context.Result = new ObjectResult(new { reason = "login_required" }) { StatusCode = 401 };
            return;
        }

        var user = await userManager.GetUserAsync(httpUser);
        if (user is null || !user.TwoFactorEnabled)
        {
            context.Result = new ObjectResult(new { reason = "otp_setup_required" }) { StatusCode = 403 };
        }
    }
}
