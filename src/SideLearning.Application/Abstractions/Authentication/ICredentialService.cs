namespace SideLearning.Application.Abstractions.Authentication;

public sealed record CredentialPrincipal(Guid UserId, string Email, IReadOnlyList<string> Roles);

public sealed record CreateCredentialResult(bool Succeeded, Guid? UserId, string? ErrorCode, IEnumerable<string>? Errors);

public interface ICredentialService
{
    Task<CreateCredentialResult> CreateAsync(string email, string password, string? displayName, CancellationToken cancellationToken);

    Task<CredentialPrincipal?> ValidateAsync(string email, string password, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}
