using SideLearning.Domain.Sessions;

namespace SideLearning.Application.Abstractions.Sessions;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken);

    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
