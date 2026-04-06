# SideLearning.Api

ASP.NET Core **host** for the HTTP API.

- **Program.cs** — Serilog, authentication/authorization, CORS, Swagger, health checks, automatic migrations, endpoint mapping.
- **Features/** — minimal API endpoint groups (`AuthEndpoints`, `TopicEndpoints`).
- **Middleware/** — global exception handling → Problem Details JSON with `code` and optional `errors`.

Run locally:

```bash
dotnet run --project src/SideLearning.Api/SideLearning.Api.csproj
```

Swagger UI is available in **Development** at `/swagger`.
