using SideLearning.Application.Features.Topics.CreateTopic;

namespace SideLearning.Application.Tests;

public sealed class CreateTopicCommandValidatorTests
{
    private readonly CreateTopicCommandValidator _validator = new();

    [Fact]
    public void Empty_name_is_invalid()
    {
        var result = _validator.Validate(new CreateTopicCommand(""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_name_is_valid()
    {
        var result = _validator.Validate(new CreateTopicCommand("Algebra"));
        Assert.True(result.IsValid);
    }
}
