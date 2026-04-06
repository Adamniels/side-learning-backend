using SideLearning.Domain.Common;

namespace SideLearning.Domain.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId, DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record UserActivatedDomainEvent(Guid UserId, DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record UserDeactivatedDomainEvent(Guid UserId, DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record UserSuspendedDomainEvent(Guid UserId, DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record UserReinstatedDomainEvent(Guid UserId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
