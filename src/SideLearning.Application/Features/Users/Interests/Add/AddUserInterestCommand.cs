namespace SideLearning.Application.Features.Users.Interests.Add;

public sealed record AddUserInterestCommand(Guid UserId, string Label, float Weight, string? Context);
