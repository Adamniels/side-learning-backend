using SideLearning.Domain.Users;

namespace SideLearning.Application.Tests;

public sealed class UserEmailTests
{
    [Fact]
    public void Create_normalizes_email_to_upper_invariant()
    {
        var email = UserEmail.Create("  Alice@example.com ");

        Assert.Equal("Alice@example.com", email.Value);
        Assert.Equal("ALICE@EXAMPLE.COM", email.NormalizedValue);
    }

    [Fact]
    public void Create_invalid_email_throws()
    {
        Assert.Throws<FormatException>(() => UserEmail.Create("not-an-email"));
    }
}
