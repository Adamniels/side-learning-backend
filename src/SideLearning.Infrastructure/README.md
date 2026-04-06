# SideLearning.Infrastructure

Technical implementations: **EF Core**, **PostgreSQL**, **ASP.NET Core Identity**, **JWT access tokens**, and **refresh token** persistence.

- **Persistence/** — `ApplicationDbContext`, EF configurations, migrations.
- **Identity/** — `ApplicationUser`, `ApplicationRole`, `IdentityCredentialService`.
- **Authentication/** — `JwtOptions`, `AuthTokenService`.
- **DependencyInjection.cs** — `AddInfrastructure`.

Design-time migrations use `DesignTimeDbContextFactory` when running `dotnet ef` commands.
