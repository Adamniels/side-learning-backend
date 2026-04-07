namespace SideLearning.Application.Features.Users.Interests.Remove;

public sealed record RemoveUserInterestCommand(Guid UserId, string Label);
