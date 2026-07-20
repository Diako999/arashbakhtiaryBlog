namespace ArashBlog.Api.Domain;

// Unlike Category/Post title fields, tags are not translated (mirrors the
// Django project's blog/translation.py, which registers title/excerpt/body
// but not tags) — one shared, language-agnostic tag vocabulary. Slug is
// unicode-safe (no ASCII transliteration) so Persian/Kurdish tag names keep
// their own characters, same as django-taggit's allow_unicode slugify.
public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
