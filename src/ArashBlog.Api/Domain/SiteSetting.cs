namespace ArashBlog.Api.Domain;

// Singleton row (Id is always 1) — mirrors the Django project's
// SingletonModel pattern. Loaded/created lazily via SingletonLoader
// rather than seeded up front, so there's exactly one code path
// ("load or create with defaults") instead of a seeder that duplicates
// the same defaults.
public class SiteSetting
{
    public int Id { get; set; }
    public string SiteName { get; set; } = "وبلاگ";
    public string? LogoUrl { get; set; }
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string InstagramUrl { get; set; } = "";
    public string TelegramUrl { get; set; } = "";
    public string TwitterUrl { get; set; } = "";
    public string LinkedinUrl { get; set; } = "";
    public string WhatsappUrl { get; set; } = "";
    public string MetaDescription { get; set; } = "";
}
