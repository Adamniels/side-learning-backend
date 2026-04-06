namespace SideLearning.Application.Abstractions.Authentication;

public sealed record AuthTokenPair(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc);

public sealed record AuthTokenPrincipal(Guid UserId, string Email, IReadOnlyList<string> Roles);

public interface IAuthTokenService
{
    Task<AuthTokenPair> IssueForPrincipalAsync(AuthTokenPrincipal principal, CancellationToken cancellationToken);

    Task<AuthTokenPair?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task RevokeRefreshAsync(string refreshToken, CancellationToken cancellationToken);
}
