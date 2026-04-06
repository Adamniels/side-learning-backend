namespace SideLearning.Application.Abstractions.Authentication;

public sealed record AuthTokenPair(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc);

public interface IAuthTokenService
{
    Task<AuthTokenPair> IssueForUserAsync(Guid userId, string email, IReadOnlyList<string> roles, CancellationToken cancellationToken);

    Task<AuthTokenPair?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task RevokeRefreshAsync(string refreshToken, CancellationToken cancellationToken);
}
