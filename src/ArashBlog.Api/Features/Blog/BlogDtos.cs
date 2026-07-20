namespace ArashBlog.Api.Features.Blog;

public record CategoryDto(string Slug, string Name);

public record PostSummaryDto(
    string Slug,
    string Title,
    string Excerpt,
    string? CoverImageUrl,
    string? CategoryName,
    string? CategorySlug,
    IReadOnlyList<string> Tags,
    DateTimeOffset? PublishedAt,
    string AuthorName);

public record PostListResponse(IReadOnlyList<PostSummaryDto> Items, int Page, int PageSize, int TotalCount);

public record CommentDto(int Id, string Name, string Body, DateTimeOffset CreatedAt);

public record PostDetailDto(
    string Slug,
    string Title,
    string BodyHtml,
    string Excerpt,
    string? CoverImageUrl,
    string? CategoryName,
    string? CategorySlug,
    IReadOnlyList<string> Tags,
    DateTimeOffset? PublishedAt,
    string AuthorName,
    int ViewCount,
    IReadOnlyList<CommentDto> Comments);

public record CreateCommentRequest(string Name, string Email, string Body);
