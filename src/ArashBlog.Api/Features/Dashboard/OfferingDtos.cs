namespace ArashBlog.Api.Features.Dashboard;

// Id is null for a new session being added, non-null for an existing one
// being edited. Any existing session not present in the submitted list on
// update gets deleted — same "sync to exactly what was submitted" behavior
// as Django's inline formset (can_delete=True) for Offering.sessions.
public record DashboardSessionDto(int? Id, DateTimeOffset StartsAt, DateTimeOffset? EndsAt, string Location, int? Capacity);

public record DashboardOfferingListItemDto(int Id, string TitleFa, string Slug, decimal? Price, string Status, DateTimeOffset CreatedAt);

public record DashboardOfferingListResponse(IReadOnlyList<DashboardOfferingListItemDto> Items);

public record DashboardEnrollmentDto(int Id, string Name, string Email, string Phone, string? SessionLabel, DateTimeOffset CreatedAt);

public record DashboardOfferingDetailDto(
    int Id,
    string Slug,
    string? CoverImageUrl,
    decimal? Price,
    string Status,
    string TitleFa,
    string TitleCkb,
    string SummaryFa,
    string SummaryCkb,
    string BodyFa,
    string BodyCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb,
    IReadOnlyList<DashboardSessionDto> Sessions,
    IReadOnlyList<DashboardEnrollmentDto> Enrollments);

public record UpsertOfferingRequest(
    string Slug,
    string? CoverImageUrl,
    decimal? Price,
    string Status,
    string TitleFa,
    string TitleCkb,
    string SummaryFa,
    string SummaryCkb,
    string BodyFa,
    string BodyCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb,
    IReadOnlyList<DashboardSessionDto> Sessions);
