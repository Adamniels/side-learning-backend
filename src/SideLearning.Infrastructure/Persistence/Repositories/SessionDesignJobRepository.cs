using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.SessionDesign;
using SideLearning.Domain.SessionDesign;

namespace SideLearning.Infrastructure.Persistence.Repositories;

public sealed class SessionDesignJobRepository(ApplicationDbContext dbContext) : ISessionDesignJobRepository
{
    private sealed class ClaimedJobRow
    {
        public Guid Id { get; set; }
    }

    public Task AddAsync(SessionDesignJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);
        return dbContext.SessionDesignJobs.AddAsync(job, cancellationToken).AsTask();
    }

    public Task<SessionDesignJob?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken)
        => dbContext.SessionDesignJobs.FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

    public Task<SessionDesignJob?> GetByIdForUserAsync(Guid userId, Guid jobId, CancellationToken cancellationToken)
        => dbContext.SessionDesignJobs.FirstOrDefaultAsync(
            x => x.Id == jobId && x.UserId == userId,
            cancellationToken);

    public async Task<Guid?> TryClaimNextQueuedAsync(CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var rows = await dbContext.Database
                .SqlQueryRaw<ClaimedJobRow>(
                    """
                    SELECT "Id" FROM session_design_jobs
                    WHERE "Status" = {0}
                    ORDER BY "CreatedAtUtc"
                    FOR UPDATE SKIP LOCKED
                    LIMIT 1
                    """,
                    (int)SessionDesignJobStatus.Queued)
                .ToListAsync(cancellationToken);

            var jobId = rows.Count > 0 ? rows[0].Id : (Guid?)null;
            if (jobId is null || jobId.Value == Guid.Empty)
            {
                await tx.CommitAsync(cancellationToken);
                return null;
            }

            var job = await dbContext.SessionDesignJobs
                .FirstAsync(x => x.Id == jobId.Value, cancellationToken);

            job.MarkRunning(DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return jobId;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> TryClaimJobByIdAsync(Guid jobId, CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var rows = await dbContext.Database
                .SqlQueryRaw<ClaimedJobRow>(
                    """
                    SELECT "Id" FROM session_design_jobs
                    WHERE "Id" = {0} AND "Status" = {1}
                    FOR UPDATE
                    """,
                    jobId,
                    (int)SessionDesignJobStatus.Queued)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                await tx.CommitAsync(cancellationToken);
                return false;
            }

            var job = await dbContext.SessionDesignJobs
                .FirstAsync(x => x.Id == jobId, cancellationToken);

            job.MarkRunning(DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
