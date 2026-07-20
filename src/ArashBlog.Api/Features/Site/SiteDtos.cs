namespace ArashBlog.Api.Features.Site;

public record SiteSettingDto(
    string SiteName,
    string? LogoUrl,
    string ContactEmail,
    string ContactPhone,
    string InstagramUrl,
    string TelegramUrl,
    string TwitterUrl,
    string LinkedinUrl,
    string WhatsappUrl,
    string MetaDescription);

public record ThemeDto(string BrandColor, string AccentColor, string DefaultMode);
