namespace ArashBlog.Api.Features.Dashboard;

public record DashboardFlatPageDto(
    int Id,
    string Slug,
    string TitleFa,
    string TitleCkb,
    string BodyFa,
    string BodyCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb);

public record UpsertFlatPageRequest(
    string TitleFa,
    string TitleCkb,
    string BodyFa,
    string BodyCkb,
    string MetaTitleFa,
    string MetaTitleCkb,
    string MetaDescriptionFa,
    string MetaDescriptionCkb);
