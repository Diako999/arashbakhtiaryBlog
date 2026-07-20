using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Leads;

// Mirrors apps/leads/{models,views}.py.
[ApiController]
[Route("api/leads")]
[RequireVisibleSection("leads")]
public class LeadsController(ApplicationDbContext db) : ControllerBase
{
    private static string PickLang(string? lang) => lang == "ckb" ? "ckb" : "fa";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeadMagnetSummaryDto>>> List([FromQuery] string? lang)
    {
        var l = PickLang(lang);
        var leadMagnets = await db.LeadMagnets
            .Where(m => m.Status == PostStatus.Published)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(leadMagnets.Select(m => new LeadMagnetSummaryDto(
            m.Slug,
            l == "ckb" ? m.TitleCkb : m.TitleFa,
            l == "ckb" ? m.DescriptionCkb : m.DescriptionFa,
            m.CoverImageUrl)).ToList());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<LeadMagnetDetailDto>> Detail(string slug, [FromQuery] string? lang)
    {
        var l = PickLang(lang);
        var leadMagnet = await db.LeadMagnets.FirstOrDefaultAsync(m => m.Slug == slug && m.Status == PostStatus.Published);
        if (leadMagnet is null) return NotFound();

        return Ok(new LeadMagnetDetailDto(
            leadMagnet.Slug,
            l == "ckb" ? leadMagnet.TitleCkb : leadMagnet.TitleFa,
            l == "ckb" ? leadMagnet.DescriptionCkb : leadMagnet.DescriptionFa,
            leadMagnet.CoverImageUrl,
            leadMagnet.FileUrl));
    }

    [HttpPost("{slug}/submit")]
    [EnableRateLimiting("comments")]
    public async Task<IActionResult> Submit(string slug, CreateSubmissionRequest request)
    {
        var leadMagnet = await db.LeadMagnets.FirstOrDefaultAsync(m => m.Slug == slug && m.Status == PostStatus.Published);
        if (leadMagnet is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { error = "invalid_submission" });
        }

        db.Submissions.Add(new Submission
        {
            LeadMagnetId = leadMagnet.Id,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
        });
        await db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { message = "submitted" });
    }
}
