using FluentValidation;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Domain.Users;

namespace SideLearning.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(
    IValidator<RegisterCommand> validator,
    ICredentialService credentialService,
    IUserRepository userRepository,
    IAuthTokenService authTokenService)
{
    public async Task<RegisterCommandResult> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var email = UserEmail.Create(command.Email);
        if (await userRepository.ExistsByNormalizedEmailAsync(email.NormalizedValue, cancellationToken))
        {
            throw new ConflictException("email_already_exists", "An account with this email already exists.");
        }

        var registration = await credentialService.CreateAsync(
            email.Value,
            command.Password,
            string.IsNullOrWhiteSpace(command.DisplayName) ? null : command.DisplayName.Trim(),
            cancellationToken);

        if (!registration.Succeeded)
        {
            if (registration.ErrorCode == RegisterErrorCodes.DuplicateEmail)
            {
                throw new ConflictException("email_already_exists", "An account with this email already exists.");
            }

            var errors = registration.Errors?.ToArray() ?? [];
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["identity"] = errors.Length > 0 ? errors : ["Registration failed."]
            });
        }

        var userId = registration.UserId!.Value;

        try
        {
            var user = User.Create(userId, email, command.DisplayName);
            await userRepository.AddAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await credentialService.DeleteAsync(userId, cancellationToken);
            throw;
        }

        var tokens = await authTokenService.IssueForPrincipalAsync(
            new AuthTokenPrincipal(userId, email.Value, []),
            cancellationToken);

        return new RegisterCommandResult(userId, tokens);
    }
}

public sealed record RegisterCommandResult(Guid UserId, AuthTokenPair Tokens);

public static class RegisterErrorCodes
{
    public const string DuplicateEmail = "DuplicateEmail";
}
