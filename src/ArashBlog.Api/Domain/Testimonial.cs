namespace ArashBlog.Api.Domain;

// AuthorName is not translated per language, matching the Django source
// (translation.py registers author_role/quote only) — a person's name
// doesn't need a fa/ckb variant.
public class Testimonial
{
    public int Id { get; set; }
    public required string AuthorName { get; set; }
    public string AuthorRoleFa { get; set; } = "";
    public string AuthorRoleCkb { get; set; } = "";
    public required string QuoteFa { get; set; }
    public required string QuoteCkb { get; set; }
    public string? PhotoUrl { get; set; }
    public string VideoUrl { get; set; } = "";

    public int? OfferingId { get; set; }
    public Offering? Offering { get; set; }

    public bool IsApproved { get; set; }
    public int Order { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
