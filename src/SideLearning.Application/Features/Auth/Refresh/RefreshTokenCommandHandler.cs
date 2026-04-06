using FluentValidation;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Common.Exceptions;

namespace SideLearning.Application.Features.Auth.Refresh;

public sealed class RefreshTokenCommandHandler(
    IValidator<RefreshTokenCommand> validator,
    IAuthTokenService authTokenService)
{
    public async Task<AuthTokenPair> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var pair = await authTokenService.RefreshAsync(command.RefreshToken, cancellationToken);
        if (pair is null)
        {
            throw new UnauthorizedAppException("invalid_refresh_token", "The refresh token is invalid or expired.");
        }

        return pair;
    }
}
