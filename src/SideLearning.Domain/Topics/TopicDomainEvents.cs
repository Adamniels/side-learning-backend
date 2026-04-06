using SideLearning.Domain.Common;

namespace SideLearning.Domain.Topics;

public sealed record TopicCreatedDomainEvent(Guid TopicId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
