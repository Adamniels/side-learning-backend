using FluentValidation;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Users.Interests.Common;

namespace SideLearning.Application.Features.Users.Interests.Update;

public sealed class UpdateUserInterestCommandHandler(
    IValidator<UpdateUserInterestCommand> validator,
    IUserRepository userRepository)
{
    public async Task<UserInterestDto> HandleAsync(UpdateUserInterestCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user_not_found", "User was not found.");
        }

        if (!user.HasInterest(command.CurrentLabel))
        {
            throw new NotFoundException("interest_not_found", "User interest was not found.");
        }

        if (!string.Equals(command.CurrentLabel, command.NewLabel, StringComparison.OrdinalIgnoreCase) &&
            user.HasInterest(command.NewLabel))
        {
            throw new ConflictException("interest_already_exists", "An interest with this label already exists.");
        }

        user.UpdateInterest(command.CurrentLabel, command.NewLabel, command.Weight, command.Context);
        await userRepository.SaveChangesAsync(cancellationToken);

        var interest = user.UserInterests
            .Single(x => string.Equals(x.Label, command.NewLabel.Trim(), StringComparison.OrdinalIgnoreCase));
        return new UserInterestDto(interest.Label, interest.Weight, interest.Context);
    }
}
