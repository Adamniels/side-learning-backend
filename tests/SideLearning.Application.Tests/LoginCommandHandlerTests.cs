using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Auth.Login;
using SideLearning.Domain.Users;

namespace SideLearning.Application.Tests;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Inactive_user_cannot_login()
    {
        var principal = new CredentialPrincipal(Guid.NewGuid(), "inactive@example.com", []);
        var credentials = new StubCredentialService(principal);
        var users = new StubUserRepository(User.Rehydrate(
            principal.UserId,
            UserEmail.Create(principal.Email),
            "Inactive",
            isActive: false,
            isSuspended: false,
            DateTimeOffset.UtcNow,
            null,
            null));
        var tokens = new StubTokenService();
        var handler = new LoginCommandHandler(new LoginCommandValidator(), credentials, users, tokens);

        var act = () => handler.HandleAsync(new LoginCommand(principal.Email, "Password1!"), CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAppException>(act);
    }

    [Fact]
    public async Task Suspended_user_cannot_login()
    {
        var principal = new CredentialPrincipal(Guid.NewGuid(), "suspended@example.com", []);
        var credentials = new StubCredentialService(principal);
        var users = new StubUserRepository(User.Rehydrate(
            principal.UserId,
            UserEmail.Create(principal.Email),
            "Suspended",
            isActive: true,
            isSuspended: true,
            DateTimeOffset.UtcNow,
            null,
            DateTimeOffset.UtcNow));
        var tokens = new StubTokenService();
        var handler = new LoginCommandHandler(new LoginCommandValidator(), credentials, users, tokens);

        var act = () => handler.HandleAsync(new LoginCommand(principal.Email, "Password1!"), CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAppException>(act);
    }

    private sealed class StubCredentialService(CredentialPrincipal principal) : ICredentialService
    {
        public Task<CreateCredentialResult> CreateAsync(string email, string password, string? displayName, CancellationToken cancellationToken)
            => Task.FromResult(new CreateCredentialResult(true, principal.UserId, null, null));

        public Task<CredentialPrincipal?> ValidateAsync(string email, string password, CancellationToken cancellationToken)
            => Task.FromResult<CredentialPrincipal?>(principal);

        public Task DeleteAsync(Guid userId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubUserRepository(User user) : IUserRepository
    {
        public Task AddAsync(User addedUser, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<User?>(id == user.Id ? user : null);

        public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
            => Task.FromResult(false);

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
