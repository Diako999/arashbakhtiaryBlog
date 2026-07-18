import nh3

# Mirrors what the TinyMCE toolbar/plugins in TINYMCE_DEFAULT_CONFIG actually
# expose (formatselect, bold/italic, lists, align, link, image, table, rtl/ltr).
# This runs server-side on every save, independent of the editor's own
# (client-side-only) toolbar restrictions — the real security boundary.
RICH_TEXT_TAGS = {
    "p", "br", "hr", "div", "span", "blockquote", "code", "pre",
    "strong", "b", "em", "i", "u",
    "h1", "h2", "h3", "h4", "h5", "h6",
    "ul", "ol", "li",
    "a", "img",
    "table", "thead", "tbody", "tr", "td", "th",
}

RICH_TEXT_ATTRIBUTES = {
    "*": {"dir"},
    "a": {"href", "target"},
    "img": {"src", "alt", "width", "height"},
    "td": {"colspan", "rowspan"},
    "th": {"colspan", "rowspan"},
    "p": {"style"},
    "span": {"style"},
    "div": {"style"},
}

RICH_TEXT_STYLE_PROPERTIES = {"color", "background-color", "text-align"}


def sanitize_html(value):
    if not value:
        return value
    return nh3.clean(
        value,
        tags=RICH_TEXT_TAGS,
        attributes=RICH_TEXT_ATTRIBUTES,
        filter_style_properties=RICH_TEXT_STYLE_PROPERTIES,
        link_rel="noopener noreferrer nofollow",
    )
