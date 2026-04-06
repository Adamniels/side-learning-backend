# SideLearning.Application

**Use cases** and application rules.

- **Features/** — feature folders with commands/queries, FluentValidation validators, and handlers.
- **Abstractions/** — `IApplicationDbContext`, `IIdentityAccountService`, `IAuthTokenService`.
- **Common/** — shared helpers (for example pagination models, slug helper, application exceptions).

Handlers are registered in `DependencyInjection.cs`. This project references **EF Core** only to express `DbSet<>` on the persistence abstraction (a pragmatic Clean Architecture trade-off).
