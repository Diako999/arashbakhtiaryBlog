using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Update-only, never create/delete — the two rows (about/contact) are
// seeded unconditionally by FlatPageSeeder and their slugs are fixed
// routes. Django left this content-editing job to the stock admin as a
// fallback; this rewrite has no admin surface, so a minimal edit screen
// exists here instead rather than leaving the content unmanageable.
[ApiController]
[Route("api/dashboard/flat-pages")]
[RequireVerifiedTwoFactor]
public class FlatPagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardFlatPageDto>>> List()
    {
        var pages = await db.FlatPages.OrderBy(p => p.Slug).ToListAsync();
        return Ok(pages.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DashboardFlatPageDto>> Detail(int id)
    {
        var page = await db.FlatPages.FindAsync(id);
        if (page is null) return NotFound();
        return Ok(ToDto(page));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DashboardFlatPageDto>> Update(int id, UpsertFlatPageRequest request)
    {
        var page = await db.FlatPages.FindAsync(id);
        if (page is null) return NotFound();

        page.TitleFa = request.TitleFa;
        page.TitleCkb = request.TitleCkb;
        page.BodyFa = PostBodySanitizer.Sanitize(request.BodyFa);
        page.BodyCkb = PostBodySanitizer.Sanitize(request.BodyCkb);
        page.MetaTitleFa = request.MetaTitleFa;
        page.MetaTitleCkb = request.MetaTitleCkb;
        page.MetaDescriptionFa = request.MetaDescriptionFa;
        page.MetaDescriptionCkb = request.MetaDescriptionCkb;
        page.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        return Ok(ToDto(page));
    }

    private static DashboardFlatPageDto ToDto(FlatPage p) => new(
        p.Id, p.Slug, p.TitleFa, p.TitleCkb, p.BodyFa, p.BodyCkb,
        p.MetaTitleFa, p.MetaTitleCkb, p.MetaDescriptionFa, p.MetaDescriptionCkb);
}
