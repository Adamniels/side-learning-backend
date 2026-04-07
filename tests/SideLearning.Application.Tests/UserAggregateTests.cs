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

    [Fact]
    public void Add_interest_sets_context_and_makes_it_available()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");

        user.AddInterest("Cybersecurity", 0.8f, "Focus on web auth attacks and mitigation.");

        var interest = Assert.Single(user.UserInterests);
        Assert.Equal("Cybersecurity", interest.Label);
        Assert.Equal(0.8f, interest.Weight);
        Assert.Equal("Focus on web auth attacks and mitigation.", interest.Context);
    }

    [Fact]
    public void Add_interest_with_duplicate_label_throws()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");
        user.AddInterest("Cybersecurity", 0.8f, "One");

        Assert.Throws<InvalidOperationException>(() =>
            user.AddInterest("cybersecurity", 0.6f, "Two"));
    }

    [Fact]
    public void Update_interest_changes_label_weight_and_context()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");
        user.AddInterest("Cybersecurity", 0.8f, "Old");

        user.UpdateInterest("Cybersecurity", "API Security", 0.9f, "JWT validation and token hygiene.");

        var interest = Assert.Single(user.UserInterests);
        Assert.Equal("API Security", interest.Label);
        Assert.Equal(0.9f, interest.Weight);
        Assert.Equal("JWT validation and token hygiene.", interest.Context);
    }

    [Fact]
    public void Remove_interest_deletes_from_collection()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("user@example.com"), "User");
        user.AddInterest("Cybersecurity", 0.8f, "Context");

        user.RemoveInterest("Cybersecurity");

        Assert.Empty(user.UserInterests);
    }
}
