using SideLearning.Domain.Common;

namespace SideLearning.Domain.SessionDesign;

public sealed class SessionDesignJob : AggregateRoot
{
    public Guid UserId { get; private set; }

    public SessionDesignJobStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public string? ResultJson { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public Guid? CreatedSessionId { get; private set; }

    private SessionDesignJob() { }

    public static SessionDesignJob CreateQueued(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Job id cannot be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        var now = DateTimeOffset.UtcNow;
        return new SessionDesignJob
        {
            Id = id,
            UserId = userId,
            Status = SessionDesignJobStatus.Queued,
            CreatedAtUtc = now
        };
    }

    public void MarkRunning(DateTimeOffset startedAtUtc)
    {
        if (Status != SessionDesignJobStatus.Queued)
        {
            throw new InvalidOperationException("Only queued jobs can be marked running.");
        }

        Status = SessionDesignJobStatus.Running;
        StartedAtUtc = startedAtUtc;
    }

    public void MarkFailed(string? errorCode, string errorMessage, DateTimeOffset completedAtUtc)
    {
        if (Status is SessionDesignJobStatus.Succeeded or SessionDesignJobStatus.Failed)
        {
            throw new InvalidOperationException("Job is already in a terminal state.");
        }

        Status = SessionDesignJobStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkSucceeded(string resultJson, Guid createdSessionId, DateTimeOffset completedAtUtc)
    {
        if (Status is SessionDesignJobStatus.Succeeded or SessionDesignJobStatus.Failed)
        {
            throw new InvalidOperationException("Job is already in a terminal state.");
        }

        Status = SessionDesignJobStatus.Succeeded;
        ResultJson = resultJson;
        CreatedSessionId = createdSessionId;
        CompletedAtUtc = completedAtUtc;
    }
}
