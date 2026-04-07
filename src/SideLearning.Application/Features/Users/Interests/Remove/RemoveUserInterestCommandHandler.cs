using FluentValidation;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;

namespace SideLearning.Application.Features.Users.Interests.Remove;

public sealed class RemoveUserInterestCommandHandler(
    IValidator<RemoveUserInterestCommand> validator,
    IUserRepository userRepository)
{
    public async Task HandleAsync(RemoveUserInterestCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user_not_found", "User was not found.");
        }

        if (!user.HasInterest(command.Label))
        {
            throw new NotFoundException("interest_not_found", "User interest was not found.");
        }

        user.RemoveInterest(command.Label);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
