using SideLearning.Application.Common;

namespace SideLearning.Application.Tests;

public sealed class SlugHelperTests
{
    [Fact]
    public void Slugify_trims_and_lowers()
    {
        var slug = SlugHelper.Slugify("  Hello World  ");
        Assert.Equal("hello-world", slug);
    }
}
