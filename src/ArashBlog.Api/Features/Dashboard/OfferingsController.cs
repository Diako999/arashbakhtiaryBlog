using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's Offering*View set — CRUD plus the
// inline Session formset and a read-only Enrollment list on the edit page.
[ApiController]
[Route("api/dashboard/offerings")]
[RequireVerifiedTwoFactor]
public class OfferingsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardOfferingListResponse>> List()
    {
        var offerings = await db.Offerings.OrderByDescending(o => o.CreatedAt).ToListAsync();
        var items = offerings.Select(o => new DashboardOfferingListItemDto(
            o.Id, o.TitleFa, o.Slug, o.Price, o.Status.ToString(), o.CreatedAt)).ToList();
        return Ok(new DashboardOfferingListResponse(items));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DashboardOfferingDetailDto>> Detail(int id)
    {
        var offering = await db.Offerings
            .Include(o => o.Sessions)
            .Include(o => o.Enrollments).ThenInclude(e => e.Session)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offering is null) return NotFound();

        return Ok(ToDetailDto(offering));
    }

    [HttpPost]
    public async Task<ActionResult<DashboardOfferingDetailDto>> Create(UpsertOfferingRequest request)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug) ? await GenerateUniqueSlugAsync(request.TitleFa) : request.Slug;
        if (await db.Offerings.AnyAsync(o => o.Slug == slug))
        {
            return Conflict(new { error = "slug_taken" });
        }

        if (!Enum.TryParse<PostStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new { error = "invalid_status" });
        }

        var offering = new Offering
        {
            Slug = slug,
            CoverImageUrl = request.CoverImageUrl,
            Price = request.Price,
            Status = status,
            TitleFa = request.TitleFa,
            TitleCkb = request.TitleCkb,
            SummaryFa = request.SummaryFa,
            SummaryCkb = request.SummaryCkb,
            BodyFa = PostBodySanitizer.Sanitize(request.BodyFa),
            BodyCkb = PostBodySanitizer.Sanitize(request.BodyCkb),
            MetaTitleFa = request.MetaTitleFa,
            MetaTitleCkb = request.MetaTitleCkb,
            MetaDescriptionFa = request.MetaDescriptionFa,
            MetaDescriptionCkb = request.MetaDescriptionCkb,
        };

        db.Offerings.Add(offering);
        SyncSessions(offering, request.Sessions);
        await db.SaveChangesAsync();

        return Ok(ToDetailDto(offering));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DashboardOfferingDetailDto>> Update(int id, UpsertOfferingRequest request)
    {
        var offering = await db.Offerings.Include(o => o.Sessions).FirstOrDefaultAsync(o => o.Id == id);
        if (offering is null) return NotFound();

        if (await db.Offerings.AnyAsync(o => o.Slug == request.Slug && o.Id != id))
        {
            return Conflict(new { error = "slug_taken" });
        }

        if (!Enum.TryParse<PostStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new { error = "invalid_status" });
        }

        offering.Slug = request.Slug;
        offering.CoverImageUrl = request.CoverImageUrl;
        offering.Price = request.Price;
        offering.Status = status;
        offering.TitleFa = request.TitleFa;
        offering.TitleCkb = request.TitleCkb;
        offering.SummaryFa = request.SummaryFa;
        offering.SummaryCkb = request.SummaryCkb;
        offering.BodyFa = PostBodySanitizer.Sanitize(request.BodyFa);
        offering.BodyCkb = PostBodySanitizer.Sanitize(request.BodyCkb);
        offering.MetaTitleFa = request.MetaTitleFa;
        offering.MetaTitleCkb = request.MetaTitleCkb;
        offering.MetaDescriptionFa = request.MetaDescriptionFa;
        offering.MetaDescriptionCkb = request.MetaDescriptionCkb;
        offering.UpdatedAt = DateTimeOffset.UtcNow;

        SyncSessions(offering, request.Sessions);
        await db.SaveChangesAsync();

        var reloaded = await db.Offerings
            .Include(o => o.Sessions)
            .Include(o => o.Enrollments).ThenInclude(e => e.Session)
            .FirstAsync(o => o.Id == id);
        return Ok(ToDetailDto(reloaded));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var offering = await db.Offerings.FindAsync(id);
        if (offering is null) return NotFound();

        db.Offerings.Remove(offering);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> GenerateUniqueSlugAsync(string titleFa)
    {
        var baseSlug = Slugifier.Slugify(titleFa);
        var slug = baseSlug;
        var counter = 1;
        while (await db.Offerings.AnyAsync(o => o.Slug == slug))
        {
            counter += 1;
            slug = $"{baseSlug}-{counter}";
        }
        return slug;
    }

    private static void SyncSessions(Offering offering, IReadOnlyList<DashboardSessionDto> submitted)
    {
        var submittedIds = submitted.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();
        foreach (var toRemove in offering.Sessions.Where(s => !submittedIds.Contains(s.Id)).ToList())
        {
            offering.Sessions.Remove(toRemove);
        }

        foreach (var dto in submitted)
        {
            if (dto.Id.HasValue)
            {
                var existing = offering.Sessions.FirstOrDefault(s => s.Id == dto.Id.Value);
                if (existing is null) continue;
                existing.StartsAt = dto.StartsAt;
                existing.EndsAt = dto.EndsAt;
                existing.Location = dto.Location;
                existing.Capacity = dto.Capacity;
            }
            else
            {
                offering.Sessions.Add(new Session
                {
                    StartsAt = dto.StartsAt,
                    EndsAt = dto.EndsAt,
                    Location = dto.Location,
                    Capacity = dto.Capacity,
                });
            }
        }
    }

    private static DashboardOfferingDetailDto ToDetailDto(Offering o) => new(
        o.Id, o.Slug, o.CoverImageUrl, o.Price, o.Status.ToString(),
        o.TitleFa, o.TitleCkb, o.SummaryFa, o.SummaryCkb, o.BodyFa, o.BodyCkb,
        o.MetaTitleFa, o.MetaTitleCkb, o.MetaDescriptionFa, o.MetaDescriptionCkb,
        o.Sessions.OrderBy(s => s.StartsAt).Select(s => new DashboardSessionDto(s.Id, s.StartsAt, s.EndsAt, s.Location, s.Capacity)).ToList(),
        o.Enrollments.OrderByDescending(e => e.CreatedAt)
            .Select(e => new DashboardEnrollmentDto(e.Id, e.Name, e.Email, e.Phone, e.Session != null ? e.Session.Location : null, e.CreatedAt))
            .ToList());
}
