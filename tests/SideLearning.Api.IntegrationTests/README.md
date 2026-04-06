# SideLearning.Api.IntegrationTests

Integration tests for the HTTP API using:

- `WebApplicationFactory<Program>`
- PostgreSQL Testcontainers (`Testcontainers.PostgreSql`)

## Run

From repo root:

```bash
make test-integration
```

or

```bash
dotnet test tests/SideLearning.Api.IntegrationTests/SideLearning.Api.IntegrationTests.csproj
```

## Notes

- Docker must be running.
- Each test run provisions an isolated PostgreSQL container.
- Tests assert endpoint behavior (status codes and response contracts).
