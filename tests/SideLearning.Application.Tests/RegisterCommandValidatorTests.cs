using SideLearning.Application.Features.Auth.Register;

namespace SideLearning.Application.Tests;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Invalid_email_is_invalid()
    {
        var result = _validator.Validate(new RegisterCommand("not-an-email", "Password1!", null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Short_password_is_invalid()
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "short", null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_request_is_valid()
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "Password1!", "User"));
        Assert.True(result.IsValid);
    }
}
