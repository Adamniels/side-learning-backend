namespace SideLearning.Application.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password, string? DisplayName);
