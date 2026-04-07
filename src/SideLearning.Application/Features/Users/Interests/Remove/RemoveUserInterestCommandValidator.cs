using FluentValidation;
using SideLearning.Domain.Users;

namespace SideLearning.Application.Features.Users.Interests.Remove;

public sealed class RemoveUserInterestCommandValidator : AbstractValidator<RemoveUserInterestCommand>
{
    public RemoveUserInterestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(UserInterest.MaxLabelLength);
    }
}
