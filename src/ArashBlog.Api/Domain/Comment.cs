namespace ArashBlog.Api.Domain;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Body { get; set; }
    public bool IsApproved { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
