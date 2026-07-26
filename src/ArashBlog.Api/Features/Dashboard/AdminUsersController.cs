using ArashBlog.Api.Common;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArashBlog.Api.Features.Dashboard;

public record AdminUserDto(string Id, string UserName, bool TwoFactorEnabled);

public record CreateAdminRequest(string Username, string Password);

// There's no self-service registration anywhere in this app (see
// AdminBootstrapper) — exactly one admin exists until another admin
// creates more through here. Any authenticated, 2FA-verified admin can
// create or remove any other; there's no separate "owner" role. The new
// admin sets up their own 2FA on first login, same flow as the original
// bootstrap admin (TwoFactorEnabled starts false here, not copied/shared).
[ApiController]
[Route("api/dashboard/admins")]
[RequireVerifiedTwoFactor]
public class AdminUsersController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<AdminUserDto>> List()
    {
        var users = userManager.Users.OrderBy(u => u.UserName).ToList();
        return Ok(users.Select(u => new AdminUserDto(u.Id, u.UserName ?? "", u.TwoFactorEnabled)).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(CreateAdminRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "username_required" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = $"{request.Username}@localhost",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = "create_failed", details = result.Errors.Select(e => e.Description) });
        }

        return Ok(new AdminUserDto(user.Id, user.UserName!, user.TwoFactorEnabled));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser?.Id == id)
        {
            return BadRequest(new { error = "cannot_delete_self" });
        }

        if (userManager.Users.Count() <= 1)
        {
            return BadRequest(new { error = "cannot_delete_last_admin" });
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        await userManager.DeleteAsync(user);
        return NoContent();
    }
}
