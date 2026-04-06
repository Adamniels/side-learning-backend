using FluentValidation;
using SideLearning.Application.Abstractions.Authentication;

namespace SideLearning.Application.Features.Auth.Revoke;

public sealed class RevokeRefreshCommandHandler(
    IValidator<RevokeRefreshCommand> validator,
    IAuthTokenService authTokenService)
{
    public async Task HandleAsync(RevokeRefreshCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        await authTokenService.RevokeRefreshAsync(command.RefreshToken, cancellationToken);
    }
}
