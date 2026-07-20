namespace ArashBlog.Api.Domain;

// FileUrl stays a plain URL field, same call as CoverImageUrl elsewhere —
// real upload validation (real-content-type sniffing, size caps) is a
// separate concern from CRUD wiring, deferred rather than half-built here.
public class LeadMagnet
{
    public int Id { get; set; }
    public required string TitleFa { get; set; }
    public required string TitleCkb { get; set; }
    public required string Slug { get; set; }
    public string DescriptionFa { get; set; } = "";
    public string DescriptionCkb { get; set; } = "";
    public string? CoverImageUrl { get; set; }
    public required string FileUrl { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public string MetaTitleFa { get; set; } = "";
    public string MetaTitleCkb { get; set; } = "";
    public string MetaDescriptionFa { get; set; } = "";
    public string MetaDescriptionCkb { get; set; } = "";

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Submission
{
    public int Id { get; set; }
    public int LeadMagnetId { get; set; }
    public LeadMagnet LeadMagnet { get; set; } = null!;
    public required string Name { get; set; }
    public required string Email { get; set; }
    public bool IsContacted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
