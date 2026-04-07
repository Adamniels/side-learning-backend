using FluentValidation;
using SideLearning.Domain.Users;

namespace SideLearning.Application.Features.Users.Interests.Update;

public sealed class UpdateUserInterestCommandValidator : AbstractValidator<UpdateUserInterestCommand>
{
    public UpdateUserInterestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentLabel)
            .NotEmpty()
            .MaximumLength(UserInterest.MaxLabelLength);
        RuleFor(x => x.NewLabel)
            .NotEmpty()
            .MaximumLength(UserInterest.MaxLabelLength);
        RuleFor(x => x.Weight)
            .InclusiveBetween(0f, 1f);
        RuleFor(x => x.Context)
            .MaximumLength(UserInterest.MaxContextLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Context));
    }
}
