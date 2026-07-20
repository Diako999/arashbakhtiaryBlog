namespace ArashBlog.Api.Domain;

public class Offering
{
    public int Id { get; set; }
    public required string TitleFa { get; set; }
    public required string TitleCkb { get; set; }
    public required string Slug { get; set; }
    public string SummaryFa { get; set; } = "";
    public string SummaryCkb { get; set; } = "";
    public string BodyFa { get; set; } = "";
    public string BodyCkb { get; set; } = "";
    public string? CoverImageUrl { get; set; }
    public decimal? Price { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public string MetaTitleFa { get; set; } = "";
    public string MetaTitleCkb { get; set; } = "";
    public string MetaDescriptionFa { get; set; } = "";
    public string MetaDescriptionCkb { get; set; } = "";

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Session
{
    public int Id { get; set; }
    public int OfferingId { get; set; }
    public Offering Offering { get; set; } = null!;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public string Location { get; set; } = "";
    public int? Capacity { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Enrollment
{
    public int Id { get; set; }
    public int OfferingId { get; set; }
    public Offering Offering { get; set; } = null!;
    public int? SessionId { get; set; }
    public Session? Session { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string Phone { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
