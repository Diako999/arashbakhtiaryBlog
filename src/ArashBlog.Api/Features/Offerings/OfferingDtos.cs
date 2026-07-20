namespace ArashBlog.Api.Features.Offerings;

public record SessionDto(int Id, DateTimeOffset StartsAt, DateTimeOffset? EndsAt, string Location, int? Capacity);

public record OfferingSummaryDto(string Slug, string Title, string Summary, string? CoverImageUrl, decimal? Price);

public record OfferingDetailDto(
    string Slug, string Title, string BodyHtml, string Summary, string? CoverImageUrl, decimal? Price,
    IReadOnlyList<SessionDto> Sessions);

public record CreateEnrollmentRequest(int? SessionId, string Name, string Email, string Phone, string Notes);
