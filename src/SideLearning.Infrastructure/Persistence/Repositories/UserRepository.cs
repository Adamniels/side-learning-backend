using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Domain.Users;

namespace SideLearning.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        return dbContext.DomainUsers.AddAsync(user, cancellationToken).AsTask();
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.DomainUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        => dbContext.DomainUsers.AnyAsync(x => x.Email.NormalizedValue == normalizedEmail, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
