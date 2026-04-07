using FluentValidation;
using SideLearning.Domain.Users;

namespace SideLearning.Application.Features.Users.Interests.Add;

public sealed class AddUserInterestCommandValidator : AbstractValidator<AddUserInterestCommand>
{
    public AddUserInterestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(UserInterest.MaxLabelLength);
        RuleFor(x => x.Weight)
            .InclusiveBetween(0f, 1f);
        RuleFor(x => x.Context)
            .MaximumLength(UserInterest.MaxContextLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Context));
    }
}
