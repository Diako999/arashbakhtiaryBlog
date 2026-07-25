using ArashBlog.Api.Features.Blog;
using ArashBlog.Api.Features.Offerings;
using ArashBlog.Api.Features.Testimonials;

namespace ArashBlog.Api.Features.Landing;

// One flat record with unused-null slots per section type, matching this
// codebase's plain-record style (no polymorphic DTOs exist anywhere else)
// — only the slot matching the section's own Type is ever populated.
public record LandingSectionDto(
    string Type,
    string Heading,
    string Subheading,
    string Body,
    string? ImageUrl,
    string PrimaryCtaText,
    string PrimaryCtaUrl,
    string SecondaryCtaText,
    string SecondaryCtaUrl,
    IReadOnlyList<OfferingSummaryDto>? Offerings,
    IReadOnlyList<PostSummaryDto>? Posts,
    IReadOnlyList<TestimonialDto>? Testimonials);
