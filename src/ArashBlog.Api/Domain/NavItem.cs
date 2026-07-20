namespace ArashBlog.Api.Domain;

// The phased-rollout switch, ported from the Django project's
// apps.navigation.NavItem.is_visible + SectionVisibleRequiredMixin. A
// single IsVisible flag is read both by the public nav listing and by
// RequireVisibleSectionAttribute at the endpoint layer (added when M3
// brings gated sections online), so "nav hidden" and "URL blocked" can't
// drift out of sync the way two separate flags could.
public class NavItem
{
    public int Id { get; set; }
    public required string TitleFa { get; set; }
    public required string TitleCkb { get; set; }

    // Stable identifier used for both display ordering and section-visibility
    // checks from protected endpoints (e.g. "blog", "offerings").
    public required string Key { get; set; }
    public required string Path { get; set; }
    public bool IsVisible { get; set; }
    public int Order { get; set; }
}
