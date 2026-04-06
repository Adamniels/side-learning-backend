using Microsoft.AspNetCore.Identity;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Features.Auth.Register;

namespace SideLearning.Infrastructure.Identity;

public sealed class IdentityCredentialService(UserManager<ApplicationUser> userManager) : ICredentialService
{
    public async Task<CreateCredentialResult> CreateAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var duplicate = result.Errors.Any(e =>
                e.Code is "DuplicateUserName" or "DuplicateEmail");
            return new CreateCredentialResult(
                false,
                null,
                duplicate ? RegisterErrorCodes.DuplicateEmail : "identity_error",
                result.Errors.Select(e => e.Description));
        }

        return new CreateCredentialResult(true, user.Id, null, null);
    }

    public async Task<CredentialPrincipal?> ValidateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new CredentialPrincipal(user.Id, user.Email ?? email, roles.ToList());
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        await userManager.DeleteAsync(user);
    }
}
