using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Auth.Register;
using SideLearning.Domain.Users;

namespace SideLearning.Application.Tests;

public sealed class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Duplicate_email_returns_conflict()
    {
        var credentials = new StubCredentialService();
        var users = new StubUserRepository { ExistingNormalizedEmails = ["USER@EXAMPLE.COM"] };
        var tokens = new StubTokenService();
        var handler = new RegisterCommandHandler(new RegisterCommandValidator(), credentials, users, tokens);

        var act = () => handler.HandleAsync(new RegisterCommand("user@example.com", "Password1!", "User"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(act);
    }

    private sealed class StubCredentialService : ICredentialService
    {
        public Task<CreateCredentialResult> CreateAsync(string email, string password, string? displayName, CancellationToken cancellationToken)
            => Task.FromResult(new CreateCredentialResult(true, Guid.NewGuid(), null, null));

        public Task<CredentialPrincipal?> ValidateAsync(string email, string password, CancellationToken cancellationToken)
            => Task.FromResult<CredentialPrincipal?>(null);

        public Task DeleteAsync(Guid userId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubUserRepository : IUserRepository
    {
        public HashSet<string> ExistingNormalizedEmails { get; init; } = [];

        public Task AddAsync(User user, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<User?>(null);

        public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
            => Task.FromResult(ExistingNormalizedEmails.Contains(normalizedEmail));

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubTokenService : IAuthTokenService
    {
        public Task<AuthTokenPair> IssueForPrincipalAsync(AuthTokenPrincipal principal, CancellationToken cancellationToken)
            => Task.FromResult(new AuthTokenPair("access", "refresh", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));

        public Task<AuthTokenPair?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
            => Task.FromResult<AuthTokenPair?>(null);

        public Task RevokeRefreshAsync(string refreshToken, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
