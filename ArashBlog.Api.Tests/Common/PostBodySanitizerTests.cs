using ArashBlog.Api.Common;
using Xunit;

namespace ArashBlog.Api.Tests.Common;

public class PostBodySanitizerTests
{
    [Fact]
    public void Strips_script_tags()
    {
        var result = PostBodySanitizer.Sanitize("<p>hi</p><script>alert(1)</script>");

        Assert.DoesNotContain("<script", result);
        Assert.Contains("<p>hi</p>", result);
    }

    [Fact]
    public void Strips_disallowed_attributes_like_onclick()
    {
        var result = PostBodySanitizer.Sanitize("<p onclick=\"alert(1)\">hi</p>");

        Assert.DoesNotContain("onclick", result);
    }

    [Fact]
    public void Adds_rel_noopener_to_links()
    {
        var result = PostBodySanitizer.Sanitize("<a href=\"https://example.com\">link</a>");

        Assert.Contains("rel=\"noopener noreferrer nofollow\"", result);
    }

    [Fact]
    public void Keeps_allowed_css_properties_but_strips_others()
    {
        var result = PostBodySanitizer.Sanitize("<p style=\"color:red;position:fixed\">hi</p>");

        Assert.Contains("color", result);
        Assert.DoesNotContain("position", result);
    }
}
