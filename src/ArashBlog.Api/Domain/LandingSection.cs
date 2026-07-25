namespace ArashBlog.Api.Domain;

public enum LandingSectionType
{
    Hero,
    OfferingsTeaser,
    PostsTeaser,
    TestimonialsTeaser,
    CtaBanner,
}

// A fixed set of 5 rows (one per LandingSectionType, enforced by a unique
// index on Type) — the admin edits content, toggles visibility, and
// reorders these relative to each other, but never creates or deletes a
// row (see LandingSectionsController: no POST, no DELETE). Different
// section types use different subsets of these shared fields (Hero uses
// heading/subheading/image/both CTAs; the teaser sections mainly use
// Heading + IsVisible, since their real content comes from the existing
// Offering/Post/Testimonial tables; CtaBanner uses heading/subheading/
// primary CTA) — a single shared-fields table was chosen over one table
// per type since the type list is small, fixed, and curated rather than
// an open-ended page-builder.
public class LandingSection
{
    public int Id { get; set; }
    public LandingSectionType Type { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; }

    public string HeadingFa { get; set; } = "";
    public string HeadingCkb { get; set; } = "";
    public string SubheadingFa { get; set; } = "";
    public string SubheadingCkb { get; set; } = "";
    public string BodyFa { get; set; } = "";
    public string BodyCkb { get; set; } = "";
    public string? ImageUrl { get; set; }

    public string PrimaryCtaTextFa { get; set; } = "";
    public string PrimaryCtaTextCkb { get; set; } = "";
    public string PrimaryCtaUrl { get; set; } = "";
    public string SecondaryCtaTextFa { get; set; } = "";
    public string SecondaryCtaTextCkb { get; set; } = "";
    public string SecondaryCtaUrl { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
