using SideLearning.Domain.Common;

namespace SideLearning.Domain.Sessions;

public sealed class Session : AggregateRoot
{
    public Guid UserId { get; private set; }

    public string Title { get; private set; } = null!;
    public string Summary { get; private set; } = null!;

    private readonly List<SessionTopic> _topics = new();
    public IReadOnlyCollection<SessionTopic> Topics => _topics.AsReadOnly();

    public string Goal { get; private set; } = null!;

    public SessionStatus Status { get; private set; }

    public int? EstimatedDurationInMinutes { get; private set; }

    public SessionContext Context { get; private set; } = null!;
    public SessionHandsOn HandsOn { get; private set; } = null!;
    public SessionReflection Reflection { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    private Session() { }

    public static Session Create(
        Guid id,
        Guid userId,
        string title,
        string summary,
        string goal,
        SessionContext context,
        SessionHandsOn handsOn,
        SessionReflection reflection,
        IEnumerable<SessionTopic>? topics = null,
        int? estimatedDurationInMinutes = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException("Summary cannot be null or whitespace.", nameof(summary));
        }

        if (string.IsNullOrWhiteSpace(goal))
        {
            throw new ArgumentException("Goal cannot be null or whitespace.", nameof(goal));
        }

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handsOn);
        ArgumentNullException.ThrowIfNull(reflection);

        if (estimatedDurationInMinutes is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(estimatedDurationInMinutes),
                "Estimated duration must be greater than zero.");
        }

        var now = DateTimeOffset.UtcNow;
        var session = new Session
        {
            Id = id,
            UserId = userId,
            Title = title.Trim(),
            Summary = summary.Trim(),
            Goal = goal.Trim(),
            Status = SessionStatus.Draft,
            EstimatedDurationInMinutes = estimatedDurationInMinutes,
            Context = context,
            HandsOn = handsOn,
            Reflection = reflection,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        if (topics is not null)
        {
            session._topics.AddRange(topics);
        }

        return session;
    }
}
