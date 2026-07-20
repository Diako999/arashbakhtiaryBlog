using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Pages;

// Mirrors apps/pages/{models,views}.py — the whole flat-pages app is one
// section, gated by a single "pages" NavItem (matches Django's
// visibility_url_name = "pages:about" covering both /about and /contact).
// No contact form — about/contact are static content pages only.
[ApiController]
[Route("api/pages")]
[RequireVisibleSection("pages")]
public class PagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<ActionResult<FlatPageDto>> Detail(string slug, [FromQuery] string? lang)
    {
        if (slug != "about" && slug != "contact") return NotFound();

        var l = lang == "ckb" ? "ckb" : "fa";
        var page = await db.FlatPages.FirstOrDefaultAsync(p => p.Slug == slug);
        if (page is null) return NotFound();

        return Ok(new FlatPageDto(page.Slug, l == "ckb" ? page.TitleCkb : page.TitleFa, l == "ckb" ? page.BodyCkb : page.BodyFa));
    }
}
