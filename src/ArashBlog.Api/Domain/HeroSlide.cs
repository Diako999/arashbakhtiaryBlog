namespace ArashBlog.Api.Domain;

// Admin-managed images for the homepage hero slider — a plain ordered list
// (no fa/ckb split, no approval flag) since a slide is just a picture with
// an optional click-through link, not language-bearing content.
public class HeroSlide
{
    public int Id { get; set; }
    public required string ImageUrl { get; set; }
    public string LinkUrl { get; set; } = "";
    public int Order { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
