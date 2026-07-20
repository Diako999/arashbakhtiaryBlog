using AngleSharp.Dom;
using Ganss.Xss;

namespace ArashBlog.Api.Common;

// Ported from the Django project's apps/core/sanitizers.py — same tag,
// attribute, and CSS-property allowlist. Must stay in sync with whatever
// the rich-text editor's toolbar exposes once the Dashboard content editor
// ships (M2). This runs server-side on every save, independent of any
// client-side editor restriction — that's the real security boundary.
public static class PostBodySanitizer
{
    private static readonly string[] AllowedTags =
    [
        "p", "br", "hr", "div", "span", "blockquote", "code", "pre",
        "strong", "b", "em", "i", "u",
        "h1", "h2", "h3", "h4", "h5", "h6",
        "ul", "ol", "li",
        "a", "img",
        "table", "thead", "tbody", "tr", "td", "th",
    ];

    private static readonly string[] AllowedAttributes =
    [
        "dir", "href", "target", "src", "alt", "width", "height", "colspan", "rowspan", "style",
    ];

    private static readonly string[] AllowedCssProperties = ["color", "background-color", "text-align"];

    public static string Sanitize(string? html)
    {
        if (string.IsNullOrEmpty(html)) return html ?? "";

        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in AllowedTags) sanitizer.AllowedTags.Add(tag);

        sanitizer.AllowedAttributes.Clear();
        foreach (var attr in AllowedAttributes) sanitizer.AllowedAttributes.Add(attr);

        sanitizer.AllowedCssProperties.Clear();
        foreach (var prop in AllowedCssProperties) sanitizer.AllowedCssProperties.Add(prop);

        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is IElement { TagName: "A" } anchor)
            {
                anchor.SetAttribute("rel", "noopener noreferrer nofollow");
            }
        };

        return sanitizer.Sanitize(html);
    }
}
