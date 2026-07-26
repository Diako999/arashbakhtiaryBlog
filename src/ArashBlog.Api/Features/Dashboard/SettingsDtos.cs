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
    string MetaDescription);

public record UpsertThemeRequest(
    string BrandColor,
    string AccentColor,
    string DefaultMode,
    string FontChoice,
    string CardStyle,
    string HeaderFooterStyle);
