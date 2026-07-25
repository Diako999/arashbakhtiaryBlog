using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Manages the fixed 5 LandingSection rows — content editing, visibility
// toggle, and reordering only. Deliberately no POST (create) and no
// DELETE: the set of section types is fixed (see LandingSection.cs), so
// omitting those two endpoints entirely is what actually enforces "admin
// edits content, doesn't invent new sections," not just a UI convention.
[ApiController]
[Route("api/dashboard/landing-sections")]
[RequireVerifiedTwoFactor]
public class LandingSectionsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardLandingSectionDto>>> List()
    {
        var sections = await db.LandingSections.OrderBy(s => s.Order).ToListAsync();
        return Ok(sections.Select(ToDto).ToList());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DashboardLandingSectionDto>> Update(int id, UpsertLandingSectionRequest request)
    {
        var section = await db.LandingSections.FindAsync(id);
        if (section is null) return NotFound();

        section.HeadingFa = request.HeadingFa;
        section.HeadingCkb = request.HeadingCkb;
        section.SubheadingFa = request.SubheadingFa;
        section.SubheadingCkb = request.SubheadingCkb;
        section.BodyFa = PostBodySanitizer.Sanitize(request.BodyFa);
        section.BodyCkb = PostBodySanitizer.Sanitize(request.BodyCkb);
        section.ImageUrl = request.ImageUrl;
        section.PrimaryCtaTextFa = request.PrimaryCtaTextFa;
        section.PrimaryCtaTextCkb = request.PrimaryCtaTextCkb;
        section.PrimaryCtaUrl = request.PrimaryCtaUrl;
        section.SecondaryCtaTextFa = request.SecondaryCtaTextFa;
        section.SecondaryCtaTextCkb = request.SecondaryCtaTextCkb;
        section.SecondaryCtaUrl = request.SecondaryCtaUrl;
        await db.SaveChangesAsync();

        return Ok(ToDto(section));
    }

    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> ToggleVisible(int id)
    {
        var section = await db.LandingSections.FindAsync(id);
        if (section is null) return NotFound();

        section.IsVisible = !section.IsVisible;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/move/{direction}")]
    public async Task<IActionResult> Move(int id, string direction)
    {
        var orderedIds = await db.LandingSections
            .OrderBy(s => s.Order)
            .Select(s => s.Id)
            .ToListAsync();

        var index = orderedIds.IndexOf(id);
        if (index < 0) return NotFound();

        var swapIndex = direction == "up" ? index - 1 : index + 1;
        if (swapIndex < 0 || swapIndex >= orderedIds.Count) return NoContent();

        (orderedIds[index], orderedIds[swapIndex]) = (orderedIds[swapIndex], orderedIds[index]);

        for (var position = 0; position < orderedIds.Count; position++)
        {
            var section = await db.LandingSections.FindAsync(orderedIds[position]);
            section!.Order = position;
        }
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static DashboardLandingSectionDto ToDto(LandingSection s) => new(
        s.Id, s.Type.ToString(), s.Order, s.IsVisible,
        s.HeadingFa, s.HeadingCkb, s.SubheadingFa, s.SubheadingCkb, s.BodyFa, s.BodyCkb, s.ImageUrl,
        s.PrimaryCtaTextFa, s.PrimaryCtaTextCkb, s.PrimaryCtaUrl,
        s.SecondaryCtaTextFa, s.SecondaryCtaTextCkb, s.SecondaryCtaUrl);
}
