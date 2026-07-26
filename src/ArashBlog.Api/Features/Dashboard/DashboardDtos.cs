using ArashBlog.Api.Features.Blog;

namespace ArashBlog.Api.Features.Dashboard;

public record OverviewDto(
    int DraftCount,
    int PublishedCount,
    IReadOnlyList<PostSummaryDto> RecentPosts);

public record TopPostDto(string Slug, string Title, int ViewCount, int BarPct);

public record CategoryStatDto(string? CategoryName, int TotalViews, int BarPct);

public record AnalyticsDto(
    int TotalViews,
    int PostCount,
    int AvgViews,
    IReadOnlyList<TopPostDto> TopPosts,
    IReadOnlyList<CategoryStatDto> CategoryStats);

public record DashboardCategoryDto(int Id, string Slug, string NameFa, string NameCkb);

public record UpsertCategoryRequest(string Slug, string NameFa, string NameCkb);

public record DashboardPostListItemDto(
    int Id, string TitleFa, string Slug, string? CategoryName, string Status, DateTimeOffset CreatedAt);

public record DashboardPostListResponse(IReadOnlyList<DashboardPostListItemDto> Items, int Page, int PageSize, int TotalCount);

public record DashboardPostDetailDto(
    int Id,
    string Slug,
    int? CategoryId,
    string Tags,
    string? CoverImageUrl,
    string? BgColor,
    string? TextColor,
    string? AccentColor,
    string Status,
    DateTimeOffset? PublishedAt,
    string TitleFa,
    string TitleCkb,
    string ExcerptFa,
    string ExcerptCkb,
    string BodyFa,
    string BodyCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb);

public record UpsertPostRequest(
    string Slug,
    int? CategoryId,
    string Tags,
    string? CoverImageUrl,
    string? BgColor,
    string? TextColor,
    string? AccentColor,
    string Status,
    DateTimeOffset? PublishedAt,
    string TitleFa,
    string TitleCkb,
    string ExcerptFa,
    string ExcerptCkb,
    string BodyFa,
    string BodyCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb);

public record DashboardCommentDto(
    int Id, int PostId, string PostTitle, string Name, string Email, string Body, bool IsApproved, DateTimeOffset CreatedAt);
