using SideLearning.Domain.Common;

namespace SideLearning.Domain.Topics;

public sealed class Topic : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Topic()
    {
    }

    public static Topic Create(string name, string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        topic.AddDomainEvent(new TopicCreatedDomainEvent(topic.Id, DateTimeOffset.UtcNow));
        return topic;
    }
}
