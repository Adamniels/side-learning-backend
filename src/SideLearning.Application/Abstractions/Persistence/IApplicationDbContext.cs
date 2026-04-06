using Microsoft.EntityFrameworkCore;
using SideLearning.Domain.Topics;

namespace SideLearning.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Topic> Topics { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
