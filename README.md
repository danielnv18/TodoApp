# Todo API (.NET 10) – Developer Guide

Minimal Todo REST API using ASP.NET Core, EF Core (SQL Server), and Swagger for exploration. Local SQL Server runs via Docker Compose.

## Prerequisites
- .NET SDK 10.0
- Docker Desktop (Compose)

## Setup: Database (local Docker)
1) Copy `.env.example` to `.env` and set a strong `SA_PASSWORD`.
2) Start SQL Server:
   ```bash
   docker compose up -d sqlserver
   docker compose ps
   ```

## Setup: Secrets and connection string
Use .NET User Secrets to keep credentials out of source control. From the repo root:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=TodoDb;User Id=sa;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
```
Replace `YOUR_PASSWORD` with the value in your `.env`. User Secrets are per-machine and not committed.

## Tooling (EF CLI)
Install EF Core CLI locally (writes to `.config/dotnet-tools.json`):
```bash
dotnet tool install dotnet-ef --version 10.0.0 --tool-manifest
```

## Database migrations
With SQL Server running and the connection string set:
```bash
dotnet ef migrations add InitialCreate   # first time
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
- `docker-compose.yml` – local SQL Server.
- `.env.example` – sample env vars for Compose (copy to `.env`).

## Coding/testing notes
- Target framework: `net10.0`, nullable enabled, implicit usings on.
- Preferred style: minimal API for now; move to controllers as complexity grows.
- Tests: plan to use xUnit with `WebApplicationFactory` under `tests/`; run via `dotnet test`.

## Troubleshooting
- If migrations fail, ensure the container is healthy (`docker compose ps`) and the secret connection string points to `localhost,1433`.
- If `dotnet-ef` is missing, rerun the tool install command above, then `dotnet tool restore` on new machines.
