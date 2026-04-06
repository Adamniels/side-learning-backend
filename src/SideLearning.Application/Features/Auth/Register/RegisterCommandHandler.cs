using FluentValidation;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Identity;
using SideLearning.Application.Common.Exceptions;

namespace SideLearning.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(
    IValidator<RegisterCommand> validator,
    IIdentityAccountService identityAccountService,
    IAuthTokenService authTokenService)
{
    public async Task<RegisterCommandResult> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var registration = await identityAccountService.RegisterAsync(
            command.Email.Trim(),
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

        var tokens = await authTokenService.IssueForUserAsync(
            registration.UserId!.Value,
            command.Email.Trim(),
            [],
            cancellationToken);

        return new RegisterCommandResult(registration.UserId.Value, tokens);
    }
}

public sealed record RegisterCommandResult(Guid UserId, AuthTokenPair Tokens);

public static class RegisterErrorCodes
{
    public const string DuplicateEmail = "DuplicateEmail";
}
