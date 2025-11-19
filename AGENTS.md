# Repository Guidelines

## Project Structure & Module Organization
- Root contains `Program.cs`, `TodoApi.csproj`, `appsettings.json`, and `PLAN.md`.
- `Properties/` holds launch settings; `obj/` and `bin/` are build outputs (do not edit).
- Add future API code under `src/` or keep minimal API endpoints in `Program.cs` until controllers are introduced.
- Tests should live under `tests/` (e.g., `tests/TodoApi.Tests/`) using xUnit.

## Build, Test, and Development Commands
- `dotnet restore` — fetch NuGet dependencies.
- `dotnet build` — compile and run analyzers.
- `dotnet run` — start the API on the configured port; use `ASPNETCORE_ENVIRONMENT=Development` for local.
- `dotnet test` — execute the test suite (add tests under `tests/`).
- `docker compose up -d` (when added) — start SQL Server container for local DB.

## Coding Style & Naming Conventions
- C# 10 / .NET 10, nullable enabled, implicit usings on.
- Prefer minimal API style for small features; move to controllers when complexity grows.
- Use PascalCase for types/methods, camelCase for locals/parameters, and snake_case for environment variables.
- Write concise comments only when behavior is non-obvious; avoid redundant narration.

## Testing Guidelines
- Use xUnit for integration/unit tests; prefer `WebApplicationFactory` for endpoint coverage.
- Name test projects `*.Tests` and test classes `{Feature}Tests`.
- Cover success and validation/error paths for each endpoint; keep tests isolated from shared state.
- Run `dotnet test` before pushing.

## Commit & Pull Request Guidelines
- Commits: clear, imperative messages (e.g., `Add todo endpoints`, `Fix null handling in Todo creation`).
- PRs: include a summary, testing notes (`dotnet test`, manual steps), and screenshots of Swagger/UI changes when applicable.
- Link issues/tasks when available; call out breaking changes or config updates (connection strings, env vars).

## Security & Configuration Tips
- Keep secrets (DB passwords, connection strings) out of Git; use `appsettings.Development.json` and user secrets or `.env` for local overrides.
- When adding Docker, ensure `SA_PASSWORD` meets SQL Server complexity; document required env vars in `README.md`.
