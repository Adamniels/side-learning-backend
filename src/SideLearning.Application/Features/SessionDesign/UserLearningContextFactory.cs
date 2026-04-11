using SideLearning.Application.Abstractions.Sessions;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.SessionDesign.Contracts;
using SideLearning.Domain.Sessions;

namespace SideLearning.Application.Features.SessionDesign;

public sealed class UserLearningContextFactory(IUserRepository userRepository, ISessionRepository sessionRepository)
    : IUserLearningContextFactory
{
    private const int MaxSessionsPerBucket = 30;

    public async Task<UserLearningContextDto> BuildAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user_not_found", "User was not found.");
        }

        var sessions = (await sessionRepository.ListByUserIdAsync(userId, cancellationToken)).ToList();

        var completed = sessions
            .Where(s => s.Status == SessionStatus.Completed)
            .OrderByDescending(s => s.UpdatedAtUtc)
            .Take(MaxSessionsPerBucket)
            .Select(s => new PastSessionItemDto
            {
                Title = s.Title,
                Summary = s.Summary,
                Topics = s.SubjectAreas.Select(a => a.Value).ToList(),
                CompletedAt = s.CompletedAtUtc?.UtcDateTime.ToString("yyyy-MM-dd"),
                LastTouchedAt = null
            })
            .ToList();

        var uncompleted = sessions
            .Where(s => s.Status is SessionStatus.Draft or SessionStatus.Ready or SessionStatus.InProgress
                or SessionStatus.Archived)
            .OrderByDescending(s => s.UpdatedAtUtc)
            .Take(MaxSessionsPerBucket)
            .Select(s => new PastSessionItemDto
            {
                Title = s.Title,
                Summary = s.Summary,
                Topics = s.SubjectAreas.Select(a => a.Value).ToList(),
                CompletedAt = null,
                LastTouchedAt = s.UpdatedAtUtc.UtcDateTime.ToString("yyyy-MM-dd")
            })
            .ToList();

        var interests = user.UserInterests
            .Select(i => new UserInterestItemDto
            {
                Label = i.Label,
                Weight = i.Weight,
                Context = i.Context ?? ""
            })
            .ToList();

        return new UserLearningContextDto
        {
            UserId = userId,
            Interests = interests,
            CompletedSessions = completed,
            UncompletedSessions = uncompleted
        };
    }
}
