namespace ArashBlog.Api.Domain;

public enum PostStatus
{
    Draft,
    Published,
}

public class Post
{
    public int Id { get; set; }
    public required string TitleFa { get; set; }
    public required string TitleCkb { get; set; }
    public required string Slug { get; set; }
    public string ExcerptFa { get; set; } = "";
    public string ExcerptCkb { get; set; } = "";
    public required string BodyFa { get; set; }
    public required string BodyCkb { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? BgColor { get; set; }
    public string? TextColor { get; set; }
    public string? AccentColor { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public string MetaTitleFa { get; set; } = "";
    public string MetaTitleCkb { get; set; } = "";
    public string MetaDescriptionFa { get; set; } = "";
    public string MetaDescriptionCkb { get; set; } = "";

    public string AuthorId { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsPublished => Status == PostStatus.Published;
}
