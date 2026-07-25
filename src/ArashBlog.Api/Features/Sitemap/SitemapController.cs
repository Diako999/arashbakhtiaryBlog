using System.Text;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Sitemap;

// Combines the Django project's four separate Sitemap classes
// (PostSitemap/OfferingSitemap/LeadMagnetSitemap/FlatPageSitemap) into one
// endpoint. Posts are always included (blog is never phased); the other
// three sections check the same NavItem.IsVisible flag the public views
// gate on, so a hidden section's URLs are never advertised to search
// engines before an admin actually publishes it.
[ApiController]
public class SitemapController(ApplicationDbContext db) : ControllerBase
{
    private record Entry(string Url, DateTimeOffset LastMod, string ChangeFreq, string Priority);

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Get()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var entries = new List<Entry>();

        // The landing page — unconditional, same as posts, since it's never
        // phased-rollout gated (see LandingController).
        entries.Add(new Entry($"{baseUrl}/fa/", DateTimeOffset.UtcNow, "weekly", "1.0"));

        var posts = await db.Posts.Where(p => p.Status == PostStatus.Published).ToListAsync();
        entries.AddRange(posts.Select(p => new Entry($"{baseUrl}/fa/blog/{Uri.EscapeDataString(p.Slug)}", p.UpdatedAt, "weekly", "0.7")));

        if (await db.NavItems.AnyAsync(n => n.Key == "offerings" && n.IsVisible))
        {
            var offerings = await db.Offerings.Where(o => o.Status == PostStatus.Published).ToListAsync();
            entries.AddRange(offerings.Select(o => new Entry($"{baseUrl}/fa/offerings/{Uri.EscapeDataString(o.Slug)}", o.UpdatedAt, "weekly", "0.6")));
        }

        if (await db.NavItems.AnyAsync(n => n.Key == "leads" && n.IsVisible))
        {
            var leadMagnets = await db.LeadMagnets.Where(l => l.Status == PostStatus.Published).ToListAsync();
            entries.AddRange(leadMagnets.Select(l => new Entry($"{baseUrl}/fa/free-resource/{Uri.EscapeDataString(l.Slug)}", l.UpdatedAt, "monthly", "0.4")));
        }

        if (await db.NavItems.AnyAsync(n => n.Key == "pages" && n.IsVisible))
        {
            var flatPages = await db.FlatPages.ToListAsync();
            entries.AddRange(flatPages.Select(p => new Entry($"{baseUrl}/fa/{p.Slug}", p.UpdatedAt, "yearly", "0.3")));
        }

        return Content(BuildXml(entries), "application/xml");
    }

    private static string BuildXml(IReadOnlyList<Entry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var entry in entries)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{System.Net.WebUtility.HtmlEncode(entry.Url)}</loc>");
            sb.AppendLine($"    <lastmod>{entry.LastMod:yyyy-MM-dd}</lastmod>");
            sb.AppendLine($"    <changefreq>{entry.ChangeFreq}</changefreq>");
            sb.AppendLine($"    <priority>{entry.Priority}</priority>");
            sb.AppendLine("  </url>");
        }
        sb.AppendLine("</urlset>");
        return sb.ToString();
    }
}
