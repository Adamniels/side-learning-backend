namespace SideLearning.Application.Features.Sessions.Common;

using SideLearning.Domain.Sessions;

public sealed record SessionDto(
    Guid Id,
    string Title,
    string Summary,
    SessionStatus Status,
    int? EstimatedDurationInMinutes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc);
