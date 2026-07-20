namespace ArashBlog.Api.Features.Leads;

public record LeadMagnetSummaryDto(string Slug, string Title, string Description, string? CoverImageUrl);

public record LeadMagnetDetailDto(string Slug, string Title, string Description, string? CoverImageUrl, string FileUrl);

public record CreateSubmissionRequest(string Name, string Email);
