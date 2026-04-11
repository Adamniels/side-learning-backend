using SideLearning.Application.Features.SessionDesign.Contracts;

namespace SideLearning.Application.Features.SessionDesign;

public interface IUserLearningContextFactory
{
    Task<UserLearningContextDto> BuildAsync(Guid userId, CancellationToken cancellationToken);
}
