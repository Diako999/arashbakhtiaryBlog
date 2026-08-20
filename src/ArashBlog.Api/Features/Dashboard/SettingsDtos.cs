namespace ArashBlog.Api.Features.Dashboard;

public record UpsertSiteSettingRequest(
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

public record DashboardHeroSlideDto(int Id, string ImageUrl, string LinkUrl, int Order);

public record UpsertHeroSlideRequest(string ImageUrl, string LinkUrl);

public record UpsertThemeRequest(string BrandColor, string AccentColor, string DefaultMode);

public record DashboardLandingPageSettingsDto(
    string HeroBadgeFa,
    string HeroBadgeCkb,
    string HeroSubtitleFa,
    string HeroSubtitleCkb,
    string HeroDescriptionFa,
    string HeroDescriptionCkb,
    string AboutRoleFa,
    string AboutRoleCkb,
    string AboutBioFa,
    string AboutBioCkb,
    string? AboutPhotoUrl,
    string AboutGithubUrl,
    string AboutYoutubeUrl);
