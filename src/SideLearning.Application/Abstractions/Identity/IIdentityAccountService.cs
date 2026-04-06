namespace SideLearning.Application.Abstractions.Identity;

public sealed record UserAuthInfo(Guid UserId, string Email, IReadOnlyList<string> Roles);

public interface IIdentityAccountService
{
    Task<RegisterUserResult> RegisterAsync(string email, string password, string? displayName, CancellationToken cancellationToken);

    Task<UserAuthInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}

public sealed record RegisterUserResult(bool Succeeded, Guid? UserId, string? ErrorCode, IEnumerable<string>? Errors);
