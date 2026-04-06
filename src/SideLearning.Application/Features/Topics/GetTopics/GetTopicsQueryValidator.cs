using FluentValidation;

namespace SideLearning.Application.Features.Topics.GetTopics;

public sealed class GetTopicsQueryValidator : AbstractValidator<GetTopicsQuery>
{
    public GetTopicsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Search));
    }
}
