namespace ArashBlog.Api.Domain;

public class Category
{
    public int Id { get; set; }
    public required string NameFa { get; set; }
    public required string NameCkb { get; set; }
    public required string Slug { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
