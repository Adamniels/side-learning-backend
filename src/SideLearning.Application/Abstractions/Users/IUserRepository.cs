using SideLearning.Domain.Users;

namespace SideLearning.Application.Abstractions.Users;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
