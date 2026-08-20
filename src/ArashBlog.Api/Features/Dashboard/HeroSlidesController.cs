using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Same list/create/update/delete/reorder shape as TestimonialsController —
// hero slides are just images with an optional link, ordered the same way.
[ApiController]
[Route("api/dashboard/hero-slides")]
[RequireVerifiedTwoFactor]
public class HeroSlidesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardHeroSlideDto>>> List()
    {
        var slides = await db.HeroSlides
            .OrderBy(h => h.Order)
            .ThenBy(h => h.CreatedAt)
            .ToListAsync();

        return Ok(slides.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<DashboardHeroSlideDto>> Create(UpsertHeroSlideRequest request)
    {
        var maxOrder = await db.HeroSlides.Select(h => (int?)h.Order).MaxAsync() ?? -1;
        var slide = new HeroSlide
        {
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            Order = maxOrder + 1,
        };
        db.HeroSlides.Add(slide);
        await db.SaveChangesAsync();

        return Ok(ToDto(slide));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DashboardHeroSlideDto>> Update(int id, UpsertHeroSlideRequest request)
    {
        var slide = await db.HeroSlides.FindAsync(id);
        if (slide is null) return NotFound();

        slide.ImageUrl = request.ImageUrl;
        slide.LinkUrl = request.LinkUrl;
        await db.SaveChangesAsync();

        return Ok(ToDto(slide));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var slide = await db.HeroSlides.FindAsync(id);
        if (slide is null) return NotFound();

        db.HeroSlides.Remove(slide);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/move/{direction}")]
    public async Task<IActionResult> Move(int id, string direction)
    {
        var orderedIds = await db.HeroSlides
            .OrderBy(h => h.Order)
            .ThenBy(h => h.CreatedAt)
            .Select(h => h.Id)
            .ToListAsync();

        var index = orderedIds.IndexOf(id);
        if (index < 0) return NotFound();

        var swapIndex = direction == "up" ? index - 1 : index + 1;
        if (swapIndex < 0 || swapIndex >= orderedIds.Count) return NoContent();

        (orderedIds[index], orderedIds[swapIndex]) = (orderedIds[swapIndex], orderedIds[index]);

        for (var position = 0; position < orderedIds.Count; position++)
        {
            var slide = await db.HeroSlides.FindAsync(orderedIds[position]);
            slide!.Order = position;
        }
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static DashboardHeroSlideDto ToDto(HeroSlide h) => new(h.Id, h.ImageUrl, h.LinkUrl, h.Order);
}
