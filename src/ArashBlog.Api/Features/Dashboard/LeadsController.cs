using System.Text;
using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's LeadMagnet*View set plus the
// Submission inbox (toggle-contacted, CSV export).
[ApiController]
[Route("api/dashboard/leads")]
[RequireVerifiedTwoFactor]
public class LeadsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardLeadMagnetListItemDto>>> List()
    {
        var leadMagnets = await db.LeadMagnets.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return Ok(leadMagnets.Select(m => new DashboardLeadMagnetListItemDto(
            m.Id, m.TitleFa, m.Slug, m.Status.ToString(), m.CreatedAt)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DashboardLeadMagnetDetailDto>> Detail(int id)
    {
        var leadMagnet = await db.LeadMagnets.FindAsync(id);
        if (leadMagnet is null) return NotFound();
        return Ok(ToDetailDto(leadMagnet));
    }

    [HttpPost]
    public async Task<ActionResult<DashboardLeadMagnetDetailDto>> Create(UpsertLeadMagnetRequest request)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug) ? await GenerateUniqueSlugAsync(request.TitleFa) : request.Slug;
        if (await db.LeadMagnets.AnyAsync(m => m.Slug == slug))
        {
            return Conflict(new { error = "slug_taken" });
        }

        if (!Enum.TryParse<PostStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new { error = "invalid_status" });
        }

        if (string.IsNullOrWhiteSpace(request.FileUrl))
        {
            return BadRequest(new { error = "file_required" });
        }

        var leadMagnet = new LeadMagnet
        {
            Slug = slug,
            CoverImageUrl = request.CoverImageUrl,
            FileUrl = request.FileUrl,
            Status = status,
            TitleFa = request.TitleFa,
            TitleCkb = request.TitleCkb,
            DescriptionFa = request.DescriptionFa,
            DescriptionCkb = request.DescriptionCkb,
            MetaTitleFa = request.MetaTitleFa,
            MetaTitleCkb = request.MetaTitleCkb,
            MetaDescriptionFa = request.MetaDescriptionFa,
            MetaDescriptionCkb = request.MetaDescriptionCkb,
        };

        db.LeadMagnets.Add(leadMagnet);
        await db.SaveChangesAsync();

        return Ok(ToDetailDto(leadMagnet));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DashboardLeadMagnetDetailDto>> Update(int id, UpsertLeadMagnetRequest request)
    {
        var leadMagnet = await db.LeadMagnets.FindAsync(id);
        if (leadMagnet is null) return NotFound();

        if (await db.LeadMagnets.AnyAsync(m => m.Slug == request.Slug && m.Id != id))
        {
            return Conflict(new { error = "slug_taken" });
        }

        if (!Enum.TryParse<PostStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new { error = "invalid_status" });
        }

        leadMagnet.Slug = request.Slug;
        leadMagnet.CoverImageUrl = request.CoverImageUrl;
        leadMagnet.FileUrl = request.FileUrl;
        leadMagnet.Status = status;
        leadMagnet.TitleFa = request.TitleFa;
        leadMagnet.TitleCkb = request.TitleCkb;
        leadMagnet.DescriptionFa = request.DescriptionFa;
        leadMagnet.DescriptionCkb = request.DescriptionCkb;
        leadMagnet.MetaTitleFa = request.MetaTitleFa;
        leadMagnet.MetaTitleCkb = request.MetaTitleCkb;
        leadMagnet.MetaDescriptionFa = request.MetaDescriptionFa;
        leadMagnet.MetaDescriptionCkb = request.MetaDescriptionCkb;
        leadMagnet.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        return Ok(ToDetailDto(leadMagnet));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var leadMagnet = await db.LeadMagnets.FindAsync(id);
        if (leadMagnet is null) return NotFound();

        db.LeadMagnets.Remove(leadMagnet);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<IReadOnlyList<DashboardSubmissionDto>>> Submissions()
    {
        var submissions = await db.Submissions
            .Include(s => s.LeadMagnet)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(submissions.Select(s => new DashboardSubmissionDto(
            s.Id, s.Name, s.Email, s.LeadMagnet.TitleFa, s.IsContacted, s.CreatedAt)).ToList());
    }

    [HttpPost("submissions/{id:int}/toggle")]
    public async Task<IActionResult> ToggleContacted(int id)
    {
        var submission = await db.Submissions.FindAsync(id);
        if (submission is null) return NotFound();

        submission.IsContacted = !submission.IsContacted;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("submissions/export")]
    public async Task<IActionResult> ExportCsv()
    {
        var submissions = await db.Submissions
            .Include(s => s.LeadMagnet)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Name,Email,Lead magnet,Contacted,Submitted at");
        foreach (var s in submissions)
        {
            csv.AppendLine(string.Join(',', new[]
            {
                CsvField(s.Name),
                CsvField(s.Email),
                CsvField(s.LeadMagnet.TitleFa),
                CsvField(s.IsContacted.ToString()),
                CsvField(s.CreatedAt.ToString("O")),
            }));
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "submissions.csv");
    }

    private static string CsvField(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private async Task<string> GenerateUniqueSlugAsync(string titleFa)
    {
        var baseSlug = Slugifier.Slugify(titleFa);
        var slug = baseSlug;
        var counter = 1;
        while (await db.LeadMagnets.AnyAsync(m => m.Slug == slug))
        {
            counter += 1;
            slug = $"{baseSlug}-{counter}";
        }
        return slug;
    }

    private static DashboardLeadMagnetDetailDto ToDetailDto(LeadMagnet m) => new(
        m.Id, m.Slug, m.CoverImageUrl, m.FileUrl, m.Status.ToString(),
        m.TitleFa, m.TitleCkb, m.DescriptionFa, m.DescriptionCkb,
        m.MetaTitleFa, m.MetaTitleCkb, m.MetaDescriptionFa, m.MetaDescriptionCkb);
}
