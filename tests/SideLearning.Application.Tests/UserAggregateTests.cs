using SideLearning.Domain.Users;

namespace SideLearning.Application.Tests;

public sealed class UserAggregateTests
{
    [Fact]
    public void New_user_can_login_by_default()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");

        Assert.True(user.CanLogin());
    }

    [Fact]
    public void Suspended_user_cannot_login()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");

        user.Suspend();

        Assert.False(user.CanLogin());
    }

    [Fact]
    public void Inactive_user_cannot_login()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");

        user.Deactivate();

        Assert.False(user.CanLogin());
    }

    [Fact]
    public void Suspend_raises_user_suspended_event()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");
        user.ClearDomainEvents();

        user.Suspend();

        Assert.Contains(user.DomainEvents, e => e is UserSuspendedDomainEvent);
    }
}
