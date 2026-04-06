# Side Learning — backend

HTTP API for the Side Learning product. It is designed as the primary backend for a **Next.js** web app and an **Expo / React Native** mobile app, with a **Clean Architecture** modular monolith layout and **JWT + refresh token** authentication.

## Architecture overview

- **Domain** — Framework-independent entities and domain rules (no EF, ASP.NET, or configuration types).
- **Application** — Use cases (commands/queries), FluentValidation validators, persistence abstractions, and auth abstractions.
- **Infrastructure** — EF Core + PostgreSQL, ASP.NET Core Identity, JWT and refresh token persistence, DI wiring.
- **Api** — Minimal API endpoints (versioned under `/api/v1`), middleware, OpenAPI/Swagger, cross-cutting HTTP concerns only.

Request flow: **Client → Api endpoint → Application handler → Domain / DbContext (via abstractions) → response.**

More detail: [docs/architecture.md](docs/architecture.md) and [docs/conventions.md](docs/conventions.md).

## Solution structure

```
SideLearning.slnx
src/
  SideLearning.Domain/
  SideLearning.Application/
  SideLearning.Infrastructure/
  SideLearning.Api/          # Host — launch this project
tests/
  SideLearning.Application.Tests/
docs/
  architecture.md
  conventions.md
```

## Required tools

- [.NET SDK](https://dotnet.microsoft.com/download) (this repo targets **net10.0**; adjust `TargetFramework` if you standardize on .NET 8/9).
- [PostgreSQL](https://www.postgresql.org/) 14+ (local or Docker).
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for the compose file below).

## Local database (Docker)

The repo includes [docker-compose.yml](docker-compose.yml): **PostgreSQL 16**, user/password `postgres`, database **`sidelearning_dev`** (same as `appsettings.Development.json`).

```bash
docker compose up -d
```

Ensure nothing else is using host port **5432** (stop other Postgres containers if needed). Data is stored in the named volume `sidelearning_pgdata`.

## How to run locally

1. Start PostgreSQL (Docker compose above, or your own instance). The Development connection string targets `sidelearning_dev`; the compose file creates that database automatically.

2. Set configuration (see below). At minimum, set **`ConnectionStrings:Database`** and a strong **`Jwt:SigningKey`** (32+ characters; use a secret manager or environment variables in production).

3. From the repository root:

```bash
dotnet restore
dotnet run --project src/SideLearning.Api/SideLearning.Api.csproj
```

4. Open Swagger UI (Development only): `https://localhost:{port}/swagger` (see `launchSettings.json` for the HTTPS port).

5. Health check: `GET /health` (no version prefix).

On startup the API applies **EF Core migrations** automatically. Ensure the database is reachable or startup will fail.

## Configuration and environment variables

Configuration merges **appsettings.json**, **appsettings.{Environment}.json**, and environment variables.

| Key | Purpose |
|-----|---------|
| `ConnectionStrings__Database` | Npgsql connection string for EF Core |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `Jwt__SigningKey` | Symmetric signing key (**required**; min 32 characters for HMAC) |
| `Jwt__AccessTokenMinutes` | Access token lifetime |
| `Jwt__RefreshTokenDays` | Refresh token lifetime |
| `Cors__AllowedOrigins__0`, `__1`, … | Allowed origins for browser clients (see CORS below) |

In Development, if `Cors:AllowedOrigins` is empty, the API allows **any origin** for local experimentation. In other environments, set explicit origins.

## Database and migrations

- **Provider:** PostgreSQL via Npgsql.
- **Migrations** live in `src/SideLearning.Infrastructure/Persistence/Migrations/`.
- **Apply** (automatic on startup) or manually:

```bash
dotnet ef database update --project src/SideLearning.Infrastructure --startup-project src/SideLearning.Api
```

**Add a new migration** after model changes:

```bash
dotnet ef migrations add YourMigrationName --project src/SideLearning.Infrastructure --startup-project src/SideLearning.Api --output-dir Persistence/Migrations
```

## API documentation (OpenAPI)

- Swagger UI is enabled in **Development** at `/swagger`.
- OpenAPI document describes versioned routes under `/api/v1`.

Use the document to generate TypeScript clients (for example OpenAPI Generator, `openapi-typescript`, or Kiota).

## Auth overview (web and mobile)

- **Access token:** JWT, sent as `Authorization: Bearer <access_token>`.
- **Refresh token:** Opaque string returned by register/login/refresh; store securely and send to `POST /api/v1/auth/refresh`. Tokens are **hashed at rest** and **rotated** on refresh.
- **Users:** ASP.NET Core Identity with **email + password**; **`UserName` equals `Email`**; **unique email** enforced (`RequireUniqueEmail` + database index).

### Recommended approach for Next.js

- **Access token:** Short-lived JWT in memory (or session storage only if you accept XSS risk).
- **Refresh token:** Prefer a **BFF** (Next.js Route Handlers): store refresh in **httpOnly, Secure, SameSite** cookies and exchange with this API from the server. The browser still calls your API with **Bearer** access tokens for same-origin or carefully CORS-configured calls.
- **Simpler local dev:** Keep refresh in memory alongside access token; do not use this pattern in production without hardening.

### Recommended approach for Expo / React Native

- Store **refresh** in **SecureStore** (or Keychain / Keystore).
- Store **access** in memory; refresh when the API returns `401` and you still have a valid refresh token.
- Always use **HTTPS** in production.

### Cookies vs unified JWT

The API is **Bearer-first** so **web and mobile share the same contract**. Web hardening (httpOnly cookies for refresh) is a **client/BFF** concern and does not require different API endpoints.

## Using this backend from Next.js

1. Set `Cors:AllowedOrigins` to your Next dev/prod origin (for example `http://localhost:3000`).
2. Call `https://your-api-host/api/v1/...` with `fetch` or your HTTP client.
3. After login/register, send `Authorization: Bearer <accessToken>` on protected routes (for example `POST /api/v1/topics`).
4. On `401`, call `POST /api/v1/auth/refresh` with the refresh token, then retry the request.

## Using this backend from Expo / React Native

1. Configure CORS only if you use a web build; native apps do not use CORS for direct HTTPS calls.
2. Use the same `/api/v1` routes and Bearer header as the web app.
3. Persist refresh tokens with **expo-secure-store** (or equivalent).

## Conventions for new features

See [docs/conventions.md](docs/conventions.md) for errors, pagination, validation, and where to place code.

## Guidance for coding agents and contributors

- **New HTTP endpoints:** Add a `MapGroup` or extend an existing one under `src/SideLearning.Api/Features/<Feature>/`, calling Application handlers only. Do not put business rules in the Api project.
- **New use cases:** Add `Features/<Feature>/<Action>/` under **Application** with command/query, `*Validator`, and `*Handler`. Register the handler in `Application/DependencyInjection.cs` if you use explicit registration.
- **Domain logic:** Put entities and domain-only rules in **Domain**. Keep them free of EF attributes, `IConfiguration`, Identity types, and HTTP types.
- **Persistence:** Add EF configurations under `Infrastructure/Persistence/Configurations/`, implement `IApplicationDbContext` on `ApplicationDbContext` if you add new aggregate roots exposed to Application.
- **What must never go in Domain:** Connection strings, DbContext, `UserManager`, JWT code, DTOs shaped only for HTTP, FluentValidation rules (validators live in Application).

Preserve existing naming, feature folders, Problem Details error shape, and `/api/v1` versioning unless the team agrees to change them.

## Project READMEs

- [src/SideLearning.Domain/README.md](src/SideLearning.Domain/README.md)
- [src/SideLearning.Application/README.md](src/SideLearning.Application/README.md)
- [src/SideLearning.Infrastructure/README.md](src/SideLearning.Infrastructure/README.md)
- [src/SideLearning.Api/README.md](src/SideLearning.Api/README.md)

## Tests

```bash
dotnet test
```

## Next recommended improvements

- Integration tests with `WebApplicationFactory` and Testcontainers PostgreSQL.
- CI pipeline (build, test, `dotnet format`).
- Email confirmation and password reset flows.
- OpenTelemetry tracing and metrics.
- Rate limiting and lockout policies tuned per environment.
- Optional OpenAPI security requirement so Swagger UI sends Bearer by default (library version differences may affect configuration).
