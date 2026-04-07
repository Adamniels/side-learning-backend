using FluentValidation;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Users.Interests.Common;

namespace SideLearning.Application.Features.Users.Interests.Add;

public sealed class AddUserInterestCommandHandler(
    IValidator<AddUserInterestCommand> validator,
    IUserRepository userRepository)
{
    public async Task<UserInterestDto> HandleAsync(AddUserInterestCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user_not_found", "User was not found.");
        }

        if (user.HasInterest(command.Label))
        {
            throw new ConflictException("interest_already_exists", "An interest with this label already exists.");
        }

        user.AddInterest(command.Label, command.Weight, command.Context);
        await userRepository.SaveChangesAsync(cancellationToken);

        var interest = user.UserInterests
            .Single(x => string.Equals(x.Label, command.Label.Trim(), StringComparison.OrdinalIgnoreCase));
        return new UserInterestDto(interest.Label, interest.Weight, interest.Context);
    }
}
