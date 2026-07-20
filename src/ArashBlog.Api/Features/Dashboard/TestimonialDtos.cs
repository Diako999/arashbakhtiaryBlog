namespace ArashBlog.Api.Features.Dashboard;

public record DashboardTestimonialDto(
    int Id,
    string AuthorName,
    string AuthorRoleFa,
    string AuthorRoleCkb,
    string QuoteFa,
    string QuoteCkb,
    string? PhotoUrl,
    string VideoUrl,
    int? OfferingId,
    bool IsApproved,
    int Order);

public record UpsertTestimonialRequest(
    string AuthorName,
    string AuthorRoleFa,
    string AuthorRoleCkb,
    string QuoteFa,
    string QuoteCkb,
    string? PhotoUrl,
    string VideoUrl,
    int? OfferingId);
