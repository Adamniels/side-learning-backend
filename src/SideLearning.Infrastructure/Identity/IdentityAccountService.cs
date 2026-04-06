using Microsoft.AspNetCore.Identity;
using SideLearning.Application.Abstractions.Identity;
using SideLearning.Application.Features.Auth.Register;

namespace SideLearning.Infrastructure.Identity;

public sealed class IdentityAccountService(UserManager<ApplicationUser> userManager) : IIdentityAccountService
{
    public async Task<RegisterUserResult> RegisterAsync(
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
            return new RegisterUserResult(
                false,
                null,
                duplicate ? RegisterErrorCodes.DuplicateEmail : "identity_error",
                result.Errors.Select(e => e.Description));
        }

        return new RegisterUserResult(true, user.Id, null, null);
    }

    public async Task<UserAuthInfo?> ValidateCredentialsAsync(
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
        return new UserAuthInfo(user.Id, user.Email ?? email, roles.ToList());
    }
}
