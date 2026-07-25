namespace ArashBlog.Api.Features.Dashboard;

public record DashboardLandingSectionDto(
    int Id,
    string Type,
    int Order,
    bool IsVisible,
    string HeadingFa,
    string HeadingCkb,
    string SubheadingFa,
    string SubheadingCkb,
    string BodyFa,
    string BodyCkb,
    string? ImageUrl,
    string PrimaryCtaTextFa,
    string PrimaryCtaTextCkb,
    string PrimaryCtaUrl,
    string SecondaryCtaTextFa,
    string SecondaryCtaTextCkb,
    string SecondaryCtaUrl);

public record UpsertLandingSectionRequest(
    string HeadingFa,
    string HeadingCkb,
    string SubheadingFa,
    string SubheadingCkb,
    string BodyFa,
    string BodyCkb,
    string? ImageUrl,
    string PrimaryCtaTextFa,
    string PrimaryCtaTextCkb,
    string PrimaryCtaUrl,
    string SecondaryCtaTextFa,
    string SecondaryCtaTextCkb,
    string SecondaryCtaUrl);
