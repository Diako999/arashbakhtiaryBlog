using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using ArashBlog.Api.Features.Blog;
using ArashBlog.Api.Features.Offerings;
using ArashBlog.Api.Features.Testimonials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Landing;

// Not gated by RequireVisibleSection — a landing page is never
// phased-rollout hidden, same as SiteController's branding/theme. Each
// teaser section (Offerings/Testimonials) additionally re-checks the
// corresponding NavItem so the landing page never advertises a section
// whose own detail pages currently 404 behind the phased-rollout gate —
// PostsTeaser needs no such check since blog is never gated.
[ApiController]
[Route("api/landing")]
public class LandingController(ApplicationDbContext db) : ControllerBase
{
    private const int TeaserCount = 3;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LandingSectionDto>>> Get([FromQuery] string? lang)
    {
        var l = lang == "ckb" ? "ckb" : "fa";

        var sections = await db.LandingSections
            .Where(s => s.IsVisible)
            .OrderBy(s => s.Order)
            .ToListAsync();

        var visibleNavKeys = await db.NavItems
            .Where(n => n.IsVisible)
            .Select(n => n.Key)
            .ToListAsync();

        var result = new List<LandingSectionDto>();

        foreach (var section in sections)
        {
            IReadOnlyList<OfferingSummaryDto>? offerings = null;
            IReadOnlyList<PostSummaryDto>? posts = null;
            IReadOnlyList<TestimonialDto>? testimonials = null;

            switch (section.Type)
            {
                case LandingSectionType.OfferingsTeaser:
                    if (!visibleNavKeys.Contains("offerings")) continue;
                    offerings = await db.Offerings
                        .Where(o => o.Status == PostStatus.Published)
                        .OrderByDescending(o => o.CreatedAt)
                        .Take(TeaserCount)
                        .Select(o => new OfferingSummaryDto(
                            o.Slug,
                            l == "ckb" ? o.TitleCkb : o.TitleFa,
                            l == "ckb" ? o.SummaryCkb : o.SummaryFa,
                            o.CoverImageUrl,
                            o.Price))
                        .ToListAsync();
                    break;

                case LandingSectionType.TestimonialsTeaser:
                    if (!visibleNavKeys.Contains("testimonials")) continue;
                    testimonials = await db.Testimonials
                        .Where(t => t.IsApproved)
                        .OrderBy(t => t.Order)
                        .ThenByDescending(t => t.CreatedAt)
                        .Take(TeaserCount)
                        .Select(t => new TestimonialDto(
                            t.AuthorName,
                            l == "ckb" ? t.AuthorRoleCkb : t.AuthorRoleFa,
                            l == "ckb" ? t.QuoteCkb : t.QuoteFa,
                            t.PhotoUrl,
                            t.VideoUrl))
                        .ToListAsync();
                    break;

                case LandingSectionType.PostsTeaser:
                    var recentPosts = await db.Posts
                        .Include(p => p.Author)
                        .Include(p => p.Category)
                        .Include(p => p.Tags)
                        .Where(p => p.Status == PostStatus.Published)
                        .OrderByDescending(p => p.PublishedAt)
                        .ThenByDescending(p => p.CreatedAt)
                        .Take(TeaserCount)
                        .ToListAsync();
                    posts = recentPosts.Select(p => new PostSummaryDto(
                        p.Slug,
                        l == "ckb" ? p.TitleCkb : p.TitleFa,
                        l == "ckb" ? p.ExcerptCkb : p.ExcerptFa,
                        p.CoverImageUrl,
                        p.BgColor,
                        p.TextColor,
                        p.AccentColor,
                        p.Category is null ? null : l == "ckb" ? p.Category.NameCkb : p.Category.NameFa,
                        p.Category?.Slug,
                        p.Tags.Select(t => t.Slug).ToList(),
                        p.PublishedAt,
                        p.Author.UserName ?? "")).ToList();
                    break;
            }

            result.Add(new LandingSectionDto(
                section.Type.ToString(),
                l == "ckb" ? section.HeadingCkb : section.HeadingFa,
                l == "ckb" ? section.SubheadingCkb : section.SubheadingFa,
                l == "ckb" ? section.BodyCkb : section.BodyFa,
                section.ImageUrl,
                l == "ckb" ? section.PrimaryCtaTextCkb : section.PrimaryCtaTextFa,
                section.PrimaryCtaUrl,
                l == "ckb" ? section.SecondaryCtaTextCkb : section.SecondaryCtaTextFa,
                section.SecondaryCtaUrl,
                offerings,
                posts,
                testimonials));
        }

        return Ok(result);
    }
}
