using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.Sessions;
using SideLearning.Domain.Sessions;

namespace SideLearning.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository(ApplicationDbContext dbContext) : ISessionRepository
{
    public Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        return dbContext.Sessions.AddAsync(session, cancellationToken).AsTask();
    }

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Sessions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Session>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
