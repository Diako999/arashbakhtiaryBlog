using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's Testimonial*View set, including the
// approve toggle and the up/down reorder that renumbers every row
// sequentially (see MoveHandler below) so `order` always reflects the
// real position even when everything started out tied at the default (0).
[ApiController]
[Route("api/dashboard/testimonials")]
[RequireVerifiedTwoFactor]
public class TestimonialsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardTestimonialDto>>> List()
    {
        var testimonials = await db.Testimonials
            .OrderBy(t => t.Order)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(testimonials.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<DashboardTestimonialDto>> Create(UpsertTestimonialRequest request)
    {
        var testimonial = new Testimonial
        {
            AuthorName = request.AuthorName,
            AuthorRoleFa = request.AuthorRoleFa,
            AuthorRoleCkb = request.AuthorRoleCkb,
            QuoteFa = request.QuoteFa,
            QuoteCkb = request.QuoteCkb,
            PhotoUrl = request.PhotoUrl,
            VideoUrl = request.VideoUrl,
            OfferingId = request.OfferingId,
        };
        db.Testimonials.Add(testimonial);
        await db.SaveChangesAsync();

        return Ok(ToDto(testimonial));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DashboardTestimonialDto>> Update(int id, UpsertTestimonialRequest request)
    {
        var testimonial = await db.Testimonials.FindAsync(id);
        if (testimonial is null) return NotFound();

        testimonial.AuthorName = request.AuthorName;
        testimonial.AuthorRoleFa = request.AuthorRoleFa;
        testimonial.AuthorRoleCkb = request.AuthorRoleCkb;
        testimonial.QuoteFa = request.QuoteFa;
        testimonial.QuoteCkb = request.QuoteCkb;
        testimonial.PhotoUrl = request.PhotoUrl;
        testimonial.VideoUrl = request.VideoUrl;
        testimonial.OfferingId = request.OfferingId;
        await db.SaveChangesAsync();

        return Ok(ToDto(testimonial));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var testimonial = await db.Testimonials.FindAsync(id);
        if (testimonial is null) return NotFound();

        db.Testimonials.Remove(testimonial);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> ToggleApproved(int id)
    {
        var testimonial = await db.Testimonials.FindAsync(id);
        if (testimonial is null) return NotFound();

        testimonial.IsApproved = !testimonial.IsApproved;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/move/{direction}")]
    public async Task<IActionResult> Move(int id, string direction)
    {
        var orderedIds = await db.Testimonials
            .OrderBy(t => t.Order)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync();

        var index = orderedIds.IndexOf(id);
        if (index < 0) return NotFound();

        var swapIndex = direction == "up" ? index - 1 : index + 1;
        if (swapIndex < 0 || swapIndex >= orderedIds.Count) return NoContent();

        (orderedIds[index], orderedIds[swapIndex]) = (orderedIds[swapIndex], orderedIds[index]);

        for (var position = 0; position < orderedIds.Count; position++)
        {
            var testimonial = await db.Testimonials.FindAsync(orderedIds[position]);
            testimonial!.Order = position;
        }
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static DashboardTestimonialDto ToDto(Testimonial t) => new(
        t.Id, t.AuthorName, t.AuthorRoleFa, t.AuthorRoleCkb, t.QuoteFa, t.QuoteCkb,
        t.PhotoUrl, t.VideoUrl, t.OfferingId, t.IsApproved, t.Order);
}
