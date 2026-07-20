using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

public record DashboardNavItemDto(int Id, string Key, string Title, bool IsVisible);

// Mirrors apps/dashboard/views.py's PagesVisibilityView / toggle_section_
// visibility — the dashboard-side half of the phased-rollout mechanism.
// "blog" is excluded: it's the one section that's always live, never part
// of the phased rollout, same as the Django TOGGLEABLE_SECTION_URL_NAMES
// whitelist leaving it out.
[ApiController]
[Route("api/dashboard/nav-items")]
[RequireVerifiedTwoFactor]
public class PagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardNavItemDto>>> List()
    {
        var items = await db.NavItems
            .Where(n => n.Key != "blog")
            .OrderBy(n => n.Order)
            .ToListAsync();

        return Ok(items.Select(n => new DashboardNavItemDto(n.Id, n.Key, n.TitleFa, n.IsVisible)).ToList());
    }

    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await db.NavItems.FirstOrDefaultAsync(n => n.Id == id && n.Key != "blog");
        if (item is null) return NotFound();

        item.IsVisible = !item.IsVisible;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
