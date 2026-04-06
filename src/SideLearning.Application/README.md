# SideLearning.Application

**Use cases** and application rules.

- **Features/** — feature folders with commands/queries, FluentValidation validators, and handlers.
- **Abstractions/** — `ICredentialService`, `IUserRepository`, `IAuthTokenService`.
- **Common/** — shared helpers (for example pagination models, slug helper, application exceptions).

Handlers are registered in `DependencyInjection.cs`.
