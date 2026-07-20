namespace ArashBlog.Api.Features.Dashboard;

public record DashboardLeadMagnetListItemDto(int Id, string TitleFa, string Slug, string Status, DateTimeOffset CreatedAt);

public record DashboardLeadMagnetDetailDto(
    int Id,
    string Slug,
    string? CoverImageUrl,
    string FileUrl,
    string Status,
    string TitleFa,
    string TitleCkb,
    string DescriptionFa,
    string DescriptionCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb);

public record UpsertLeadMagnetRequest(
    string Slug,
    string? CoverImageUrl,
    string FileUrl,
    string Status,
    string TitleFa,
    string TitleCkb,
    string DescriptionFa,
    string DescriptionCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb);

public record DashboardSubmissionDto(int Id, string Name, string Email, string LeadMagnetTitle, bool IsContacted, DateTimeOffset CreatedAt);
