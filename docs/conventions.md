# Conventions

## HTTP API

- **Version prefix:** All JSON business endpoints are under **`/api/v1`**.
- **Health:** `GET /health` (unversioned).
- **Status codes:** Use standard semantics (201 for create, 204 for revoke, 400 validation, 401 auth, 404 missing resource, 409 conflicts).

## Error responses (Problem Details)

Clients should parse **`application/problem+json`** (or JSON compatible with `ProblemDetails`).

Stable shape:

- `title`, `detail`, `status`, `type` — RFC 7807-style fields.
- **`code`** (string) — stable machine-readable code for TypeScript `switch` / i18n keys.
- **`errors`** (optional object, string → string[]) — validation and field-level messages.

Examples:

- FluentValidation failures → **400**, `code`: `validation_failed`.
- Duplicate email on register → **409**, `code`: `email_already_exists`.
- Invalid login → **401**, `code`: `invalid_credentials`.

## Pagination

List endpoints use query parameters **`page`** (1-based) and **`pageSize`** (capped in validators, typically max **100**).

Response envelope:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

## Sorting and filtering

- Prefer **explicit query parameters** per resource (`search`, `sort`, …).
- Avoid generic JSON query DSLs unless requirements justify the complexity.

## Validation

- **FluentValidation** validators live next to commands/queries in **Application**.
- Handlers call `ValidateAndThrowAsync`; the Api middleware maps FluentValidation’s `ValidationException` to Problem Details.

## Idempotency

`POST` creates are not idempotent unless documented otherwise. For future payment-like or retry-heavy operations, consider an **`Idempotency-Key`** header and server-side deduplication (not implemented in the scaffold).

## Where to add code

| Concern | Location |
|---------|-----------|
| New route | `src/SideLearning.Api/Features/<Feature>/` |
| Use case + validation | `src/SideLearning.Application/Features/<Feature>/` |
| Entity / domain rule | `src/SideLearning.Domain/` |
| EF mapping / migrations | `src/SideLearning.Infrastructure/Persistence/` |

## What must never live in Domain

- EF Core attributes, `DbContext`, migrations, or SQL.
- ASP.NET Core types (`HttpContext`, `ProblemDetails`, …).
- Identity (`UserManager`, `IdentityUser`, …) and JWT signing.
- Application DTOs or HTTP request/response models.

## Naming

- Commands: `VerbNounCommand` (for example `CreateTopicCommand`).
- Handlers: `CreateTopicCommandHandler`.
- Validators: `CreateTopicCommandValidator`.
- Endpoints: `*Endpoints` static class with `Map*Endpoints` extension on `RouteGroupBuilder`.
