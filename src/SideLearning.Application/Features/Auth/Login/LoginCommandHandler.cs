using FluentValidation;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Identity;
using SideLearning.Application.Common.Exceptions;

namespace SideLearning.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    IValidator<LoginCommand> validator,
    IIdentityAccountService identityAccountService,
    IAuthTokenService authTokenService)
{
    public async Task<AuthTokenPair> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await identityAccountService.ValidateCredentialsAsync(
            command.Email.Trim(),
            command.Password,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAppException("invalid_credentials", "Invalid email or password.");
        }

        return await authTokenService.IssueForUserAsync(user.UserId, user.Email, user.Roles, cancellationToken);
    }
}
