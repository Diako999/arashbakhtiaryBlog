using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Testimonials;

public record TestimonialDto(string AuthorName, string AuthorRole, string Quote, string? PhotoUrl, string VideoUrl);

// Mirrors apps/testimonials/{models,views}.py.
[ApiController]
[Route("api/testimonials")]
[RequireVisibleSection("testimonials")]
public class TestimonialsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TestimonialDto>>> List([FromQuery] string? lang)
    {
        var l = lang == "ckb" ? "ckb" : "fa";
        var testimonials = await db.Testimonials
            .Where(t => t.IsApproved)
            .OrderBy(t => t.Order)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(testimonials.Select(t => new TestimonialDto(
            t.AuthorName,
            l == "ckb" ? t.AuthorRoleCkb : t.AuthorRoleFa,
            l == "ckb" ? t.QuoteCkb : t.QuoteFa,
            t.PhotoUrl,
            t.VideoUrl)).ToList());
    }
}
