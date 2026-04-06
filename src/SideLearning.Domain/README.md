# SideLearning.Domain

Framework-independent **domain model** for Side Learning.

- Put **entities**, **value objects**, and domain-only invariants here.
- Do **not** reference EF Core, ASP.NET Core, Identity, or configuration APIs.

Example: `Users/User.cs`.

## Domain foundation

- `Common/Entity.cs` provides identity-based equality and domain event collection.
- `Common/AggregateRoot.cs` marks aggregate roots (for example `User`).
- `Common/IDomainEvent.cs` is the contract for domain events raised by aggregates.
