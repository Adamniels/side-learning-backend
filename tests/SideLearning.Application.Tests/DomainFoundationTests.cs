using SideLearning.Domain.Users;

namespace SideLearning.Application.Tests;

public sealed class DomainFoundationTests
{
    [Fact]
    public void Entity_equality_is_based_on_type_and_id()
    {
        var id = Guid.NewGuid();
        var userA = User.Rehydrate(id, UserEmail.Create("same@example.com"), "A", true, false, DateTimeOffset.UtcNow, null, null);
        var userB = User.Rehydrate(id, UserEmail.Create("other@example.com"), "B", true, false, DateTimeOffset.UtcNow, null, null);
        var otherUser = User.Rehydrate(Guid.NewGuid(), UserEmail.Create("third@example.com"), "C", true, false, DateTimeOffset.UtcNow, null, null);

        Assert.True(userA.Equals(userB));
        Assert.False(userA.Equals(otherUser));
    }

    [Fact]
    public void Aggregate_records_domain_events_and_can_clear_them()
    {
        var user = User.Create(Guid.NewGuid(), UserEmail.Create("events@example.com"), "Events");

        Assert.NotEmpty(user.DomainEvents);
        user.ClearDomainEvents();
        Assert.Empty(user.DomainEvents);
    }
}
