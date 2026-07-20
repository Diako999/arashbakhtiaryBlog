using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Offerings;

// Mirrors apps/offerings/{models,views}.py. Gated by RequireVisibleSection
// — the phased-rollout mechanism — so hitting these URLs directly 404s
// exactly like the public nav link disappearing, until an admin flips the
// "offerings" NavItem visible from the dashboard's Pages screen.
[ApiController]
[Route("api/offerings")]
[RequireVisibleSection("offerings")]
public class OfferingsController(ApplicationDbContext db) : ControllerBase
{
    private static string PickLang(string? lang) => lang == "ckb" ? "ckb" : "fa";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OfferingSummaryDto>>> List([FromQuery] string? lang)
    {
        var l = PickLang(lang);
        var offerings = await db.Offerings
            .Where(o => o.Status == PostStatus.Published)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(offerings.Select(o => new OfferingSummaryDto(
            o.Slug,
            l == "ckb" ? o.TitleCkb : o.TitleFa,
            l == "ckb" ? o.SummaryCkb : o.SummaryFa,
            o.CoverImageUrl,
            o.Price)).ToList());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<OfferingDetailDto>> Detail(string slug, [FromQuery] string? lang)
    {
        var l = PickLang(lang);
        var offering = await db.Offerings
            .Include(o => o.Sessions)
            .FirstOrDefaultAsync(o => o.Slug == slug && o.Status == PostStatus.Published);

        if (offering is null) return NotFound();

        var sessions = offering.Sessions
            .OrderBy(s => s.StartsAt)
            .Select(s => new SessionDto(s.Id, s.StartsAt, s.EndsAt, s.Location, s.Capacity))
            .ToList();

        return Ok(new OfferingDetailDto(
            offering.Slug,
            l == "ckb" ? offering.TitleCkb : offering.TitleFa,
            l == "ckb" ? offering.BodyCkb : offering.BodyFa,
            l == "ckb" ? offering.SummaryCkb : offering.SummaryFa,
            offering.CoverImageUrl,
            offering.Price,
            sessions));
    }

    [HttpPost("{slug}/enroll")]
    [EnableRateLimiting("comments")]
    public async Task<IActionResult> Enroll(string slug, CreateEnrollmentRequest request)
    {
        var offering = await db.Offerings.FirstOrDefaultAsync(o => o.Slug == slug && o.Status == PostStatus.Published);
        if (offering is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { error = "invalid_enrollment" });
        }

        if (request.SessionId.HasValue && !await db.Sessions.AnyAsync(s => s.Id == request.SessionId && s.OfferingId == offering.Id))
        {
            return BadRequest(new { error = "invalid_session" });
        }

        db.Enrollments.Add(new Enrollment
        {
            OfferingId = offering.Id,
            SessionId = request.SessionId,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Notes = request.Notes.Trim(),
        });
        await db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { message = "submitted" });
    }
}
