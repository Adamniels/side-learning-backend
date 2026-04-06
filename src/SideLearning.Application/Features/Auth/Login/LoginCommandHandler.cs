using FluentValidation;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;

namespace SideLearning.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    IValidator<LoginCommand> validator,
    ICredentialService credentialService,
    IUserRepository userRepository,
    IAuthTokenService authTokenService)
{
    public async Task<AuthTokenPair> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var principal = await credentialService.ValidateAsync(
            command.Email.Trim(),
            command.Password,
            cancellationToken);

        if (principal is null)
        {
            throw new UnauthorizedAppException("invalid_credentials", "Invalid email or password.");
        }

        var user = await userRepository.GetByIdAsync(principal.UserId, cancellationToken);
        if (user is null || !user.CanLogin())
        {
            throw new UnauthorizedAppException("invalid_credentials", "Invalid email or password.");
        }

        return await authTokenService.IssueForPrincipalAsync(
            new AuthTokenPrincipal(principal.UserId, principal.Email, principal.Roles),
            cancellationToken);
    }
}
