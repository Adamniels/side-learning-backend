using FluentValidation;

namespace SideLearning.Application.Features.Auth.Revoke;

public sealed class RevokeRefreshCommandValidator : AbstractValidator<RevokeRefreshCommand>
{
    public RevokeRefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
