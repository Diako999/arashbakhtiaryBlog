using System.Text.RegularExpressions;

namespace ArashBlog.Api.Common;

// Rough equivalent of Django's slugify(value, allow_unicode=True): lowercase,
// collapse whitespace/separators to hyphens, strip anything that isn't a
// letter/digit/hyphen/underscore in ANY script — so Persian/Kurdish text
// keeps its own characters instead of being transliterated away.
public static partial class Slugifier
{
    [GeneratedRegex(@"[^\p{L}\p{N}_-]")]
    private static partial Regex NonWordChars();

    [GeneratedRegex(@"[-\s]+")]
    private static partial Regex WhitespaceRun();

    public static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        normalized = NonWordChars().Replace(normalized, " ");
        normalized = WhitespaceRun().Replace(normalized, "-").Trim('-');
        return normalized.Length > 0 ? normalized : "n-a";
    }
}
