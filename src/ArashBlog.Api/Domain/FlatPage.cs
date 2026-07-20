namespace ArashBlog.Api.Domain;

// Only ever two rows ("about", "contact") — fixed routes, not a general
// CMS. Slug is immutable by convention (the dashboard only ever updates
// an existing row, never creates/deletes), matching the Django source's
// literal /about/ and /contact/ URL registration rather than a generic
// <slug>/ catch-all.
public class FlatPage
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string TitleFa { get; set; }
    public required string TitleCkb { get; set; }
    public string BodyFa { get; set; } = "";
    public string BodyCkb { get; set; } = "";
    public string MetaTitleFa { get; set; } = "";
    public string MetaTitleCkb { get; set; } = "";
    public string MetaDescriptionFa { get; set; } = "";
    public string MetaDescriptionCkb { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
