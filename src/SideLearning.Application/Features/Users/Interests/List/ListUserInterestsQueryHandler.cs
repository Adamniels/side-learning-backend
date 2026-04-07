using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Users.Interests.Common;

namespace SideLearning.Application.Features.Users.Interests.List;

public sealed class ListUserInterestsQueryHandler(IUserRepository userRepository)
{
    public async Task<IReadOnlyCollection<UserInterestDto>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user_not_found", "User was not found.");
        }

        return user.UserInterests
            .Select(x => new UserInterestDto(x.Label, x.Weight, x.Context))
            .ToArray();
    }
}
