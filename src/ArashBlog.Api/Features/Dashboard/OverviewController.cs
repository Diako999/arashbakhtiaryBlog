using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's OverviewView/AnalyticsView. Offering/
// Submission counts are left out of Overview until M3 brings those domains
// online — showing a permanent zero for something that doesn't exist yet
// would be misleading, so the field is simply absent rather than faked.
[ApiController]
[Route("api/dashboard")]
[RequireVerifiedTwoFactor]
public class OverviewController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<OverviewDto>> Overview()
    {
        var draftCount = await db.Posts.CountAsync(p => p.Status == PostStatus.Draft);
        var publishedCount = await db.Posts.CountAsync(p => p.Status == PostStatus.Published);

        var recent = await db.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentDtos = recent.Select(p => new PostSummaryDto(
            p.Slug, p.TitleFa, p.ExcerptFa, p.CoverImageUrl,
            p.BgColor, p.TextColor, p.AccentColor,
            p.Category?.NameFa, p.Category?.Slug,
            p.Tags.Select(t => t.Slug).ToList(),
            p.PublishedAt, p.Author.UserName ?? "")).ToList();

        return Ok(new OverviewDto(draftCount, publishedCount, recentDtos));
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsDto>> Analytics()
    {
        var published = db.Posts.Where(p => p.Status == PostStatus.Published);

        var totalViews = await published.SumAsync(p => (int?)p.ViewCount) ?? 0;
        var postCount = await published.CountAsync();
        var avgViews = postCount > 0 ? (int)Math.Round(totalViews / (double)postCount) : 0;

        var topPosts = await published
            .Include(p => p.Category)
            .OrderByDescending(p => p.ViewCount)
            .Take(10)
            .ToListAsync();
        var maxPostViews = topPosts.Count > 0 ? Math.Max(topPosts.Max(p => p.ViewCount), 1) : 1;
        var topPostDtos = topPosts
            .Select(p => new TopPostDto(p.Slug, p.TitleFa, p.ViewCount, (int)Math.Round(p.ViewCount / (double)maxPostViews * 100)))
            .ToList();

        var categoryStats = await published
            .Include(p => p.Category)
            .GroupBy(p => p.Category!.NameFa)
            .Select(g => new { CategoryName = g.Key, TotalViews = g.Sum(p => p.ViewCount) })
            .OrderByDescending(g => g.TotalViews)
            .Take(10)
            .ToListAsync();
        var maxCategoryViews = categoryStats.Count > 0 ? Math.Max(categoryStats.Max(c => c.TotalViews), 1) : 1;
        var categoryStatDtos = categoryStats
            .Select(c => new CategoryStatDto(c.CategoryName, c.TotalViews, (int)Math.Round(c.TotalViews / (double)maxCategoryViews * 100)))
            .ToList();

        return Ok(new AnalyticsDto(totalViews, postCount, avgViews, topPostDtos, categoryStatDtos));
    }
}
