namespace SideLearning.Application.Features.Users.Interests.Update;

public sealed record UpdateUserInterestCommand(Guid UserId, string CurrentLabel, string NewLabel, float Weight, string? Context);
