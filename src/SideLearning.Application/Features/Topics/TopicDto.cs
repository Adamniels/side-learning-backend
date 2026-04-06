namespace SideLearning.Application.Features.Topics;

public sealed record TopicDto(Guid Id, string Name, string Slug, DateTimeOffset CreatedAtUtc);
