using SideLearning.Domain.SessionDesign;

namespace SideLearning.Application.Abstractions.SessionDesign;

public interface ISessionDesignJobRepository
{
    Task AddAsync(SessionDesignJob job, CancellationToken cancellationToken);

    Task<SessionDesignJob?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken);

    Task<SessionDesignJob?> GetByIdForUserAsync(Guid userId, Guid jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Claims the next queued job (PostgreSQL FOR UPDATE SKIP LOCKED) and marks it Running.
    /// </summary>
    Task<Guid?> TryClaimNextQueuedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Claims a specific job if it is Queued (row FOR UPDATE).
    /// </summary>
    Task<bool> TryClaimJobByIdAsync(Guid jobId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
