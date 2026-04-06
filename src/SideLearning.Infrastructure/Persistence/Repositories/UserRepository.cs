using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Domain.Users;

namespace SideLearning.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var record = new DomainUserRecord
        {
            Id = user.Id,
            Email = user.Email.Value,
            NormalizedEmail = user.Email.NormalizedValue,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            IsSuspended = user.IsSuspended,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc,
            SuspendedAtUtc = user.SuspendedAtUtc
        };

        return dbContext.DomainUsers.AddAsync(record, cancellationToken).AsTask();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await dbContext.DomainUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        return User.Rehydrate(
            record.Id,
            UserEmail.Create(record.Email),
            record.DisplayName,
            record.IsActive,
            record.IsSuspended,
            record.CreatedAtUtc,
            record.UpdatedAtUtc,
            record.SuspendedAtUtc);
    }

    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        => dbContext.DomainUsers.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
