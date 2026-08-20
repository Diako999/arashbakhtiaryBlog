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
    string YoutubeUrl,
    string MetaDescription);

public record HeroSlideDto(string ImageUrl, string LinkUrl);

public record ThemeDto(string BrandColor, string AccentColor, string DefaultMode);

// Language-resolved for the public site — empty Hero*/AboutBio strings mean
// "admin hasn't customized this yet"; the frontend falls back to its i18n
// copy the same way it already did for HeroDescription before this existed.
public record LandingPageSettingsDto(
    string HeroBadge,
    string HeroSubtitle,
    string HeroDescription,
    string AboutRole,
    string AboutBio,
    string? AboutPhotoUrl,
    string AboutGithubUrl,
    string AboutYoutubeUrl);
