using ArashBlog.Api.Common;
using Xunit;

namespace ArashBlog.Api.Tests.Common;

public class SlugifierTests
{
    [Fact]
    public void Keeps_persian_characters_instead_of_transliterating()
    {
        var slug = Slugifier.Slugify("نخستین نوشته وبلاگ");

        Assert.Equal("نخستین-نوشته-وبلاگ", slug);
    }

    [Fact]
    public void Lowercases_and_hyphenates_latin_input()
    {
        var slug = Slugifier.Slugify("Hello   World");

        Assert.Equal("hello-world", slug);
    }

    [Fact]
    public void Strips_punctuation()
    {
        var slug = Slugifier.Slugify("Q&A: what now?!");

        Assert.Equal("q-a-what-now", slug);
    }
}
