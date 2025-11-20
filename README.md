# Todo API (.NET 10) – Developer Guide

Minimal Todo REST API using ASP.NET Core, EF Core (SQLite), and Swagger for exploration. Local dev uses a SQLite file (`todo.db`)—no Docker required.

## Prerequisites
- .NET SDK 10.0

## Setup: Database (SQLite)
- Default connection string: `Data Source=todo.db` (in `appsettings.json`). No environment variables needed.
- To change the file path, override the connection string via user secrets or env var `ConnectionStrings__DefaultConnection`.

## Tooling (EF CLI)
Install EF Core CLI locally (writes to `.config/dotnet-tools.json`):
```bash
dotnet tool install dotnet-ef --version 10.0.0 --tool-manifest
```

## Database migrations
SQLite requires minimal setup:
```bash
dotnet ef migrations add InitialCreate   # if no migrations yet
dotnet ef database update
```

## Run the API
```bash
dotnet run
```
- Swagger UI: `http://localhost:5167/swagger` (or the HTTPS port in launchSettings).
- Health check not added yet; a simple root `GET /` returns a status message.

## Project structure
- `Program.cs` – minimal API setup, Swagger, DbContext registration.
- `Models/` – entity classes (e.g., `Todo`).
- `Contracts/` – request/response DTOs.
- `Data/` – EF Core DbContext.
- `.env.example` – placeholder (SQLite does not require env vars by default).

## Coding/testing notes
- Target framework: `net10.0`, nullable enabled, implicit usings on.
- Preferred style: minimal API for now; move to controllers as complexity grows.
- Tests: plan to use xUnit with `WebApplicationFactory` under `tests/`; run via `dotnet test`.

## Troubleshooting
- If migrations fail, ensure the connection string points to a writable path (default `todo.db` in project root).
- If `dotnet-ef` is missing, rerun the tool install command above, then `dotnet tool restore` on new machines.
