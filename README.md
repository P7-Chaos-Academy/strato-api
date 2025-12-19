# Strato API

Backend API for the Strato cluster management system. Built with .NET 8 and PostgreSQL.

## Prerequisites

- .NET 8 SDK
- PostgreSQL (docker-compose recommended)

## Getting Started

```bash
dotnet restore
dotnet build
dotnet run
```

## Configuration

Set secrets for development:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=strato;Username=postgres;Password=postgres;"
dotnet user-secrets set "JwtSettings:SecretKey" "<your-secret>"
```

## Database Migrations

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## API Endpoints

- `/api/auth` - Authentication (JWT + API keys)
- `/api/cluster` - Cluster management
- `/api/jobs` - Job management
- `/api/metrics` - Metrics
- `/health` - Health check

## Docker

```bash
docker-compose up --build
```
