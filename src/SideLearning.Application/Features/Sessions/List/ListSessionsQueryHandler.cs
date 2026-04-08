using SideLearning.Application.Abstractions.Sessions;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Sessions.Common;
using SideLearning.Application.Abstractions.Users;

namespace SideLearning.Application.Features.Sessions.List;

public sealed class ListSessionsQueryHandler(ISessionRepository sessionRepository, IUserRepository userRepository)
{
    public async Task<IReadOnlyCollection<SessionDto>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user_not_found", "User was not found.");
        }

        var sessions = await sessionRepository.ListByUserIdAsync(userId, cancellationToken);

        return sessions
            .Select(x => new SessionDto(
                x.Id,
                x.Title,
                x.Summary,
                x.Status,
                x.EstimatedDurationInMinutes,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.StartedAtUtc,
                x.CompletedAtUtc))
            .ToArray();
    }
}
