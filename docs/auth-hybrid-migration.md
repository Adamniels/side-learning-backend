# Auth Hybrid Migration Notes

## Boundary model

- Identity remains the credential and security subsystem (password, hashing, verification, lockout, normalization).
- `domain_users` is the business aggregate for user state and behavior.
- Application coordinates both through ports (`ICredentialService`, `IUserRepository`, `IAuthTokenService`).

## Link strategy

- `domain_users.Id` reuses `AspNetUsers.Id` (same GUID).
- Foreign key `domain_users.Id -> AspNetUsers.Id` with cascade delete.

## Migration sequence

1. Create `domain_users` table and unique index on `NormalizedEmail`.
2. Backfill rows from `AspNetUsers` for existing users.
3. Keep Identity and refresh token tables untouched.
4. Route register/login through hybrid orchestration.

## Register flow safety

- Credentials are created first in Identity.
- Domain user is then created in `domain_users`.
- If domain persistence fails, credential create is compensated by deleting the identity user.

## Operational cautions

- Apply migration before deploying code paths that require `domain_users`.
- Verify no duplicate normalized emails exist in legacy data before production rollout.
- Keep auth error contract parity (`invalid_credentials`, `invalid_refresh_token`, `email_already_exists`).
