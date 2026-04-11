using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SideLearning.Domain.SessionDesign;
using SideLearning.Domain.Sessions;
using SideLearning.Domain.Users;
using SideLearning.Infrastructure.Identity;

namespace SideLearning.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionDesignJob> SessionDesignJobs => Set<SessionDesignJob>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
