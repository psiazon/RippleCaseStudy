# Ripple Ticketing Case Study

This solution implements a simplified event ticketing system using C#, ASP.NET Core controllers, Clean Architecture, CQRS with MediatR, FluentValidation, EF Core 10, SQL Server Express, JWT bearer authentication, role/policy authorization, rate limiting, CORS restrictions, centralized Serilog logging, safe error responses, request body limits, and xUnit tests.

## Projects

- `Ripple.EventManagement.Api` exposes event CRUD endpoints.
- `Ripple.EventManagement.Application` contains CQRS commands, queries, validators, DTOs, and MediatR validation pipeline behavior.
- `Ripple.EventManagement.Domain` contains event aggregate entities.
- `Ripple.EventManagement.Infrastructure` contains EF Core SQL Server persistence.
- `Ripple.TicketManagement.Api` exposes ticket purchase, availability, and reporting endpoints.
- `Ripple.TicketManagement.Application` contains ticket CQRS use cases and abstractions.
- `Ripple.TicketManagement.Domain` contains ticket order and inventory entities.
- `Ripple.TicketManagement.Infrastructure` contains EF Core persistence and the HTTP client that consumes Event Management API endpoints.
- `tests/*` contains xUnit unit tests.

## Setup

1. Install .NET 10 SDK and SQL Server Express.
2. Update both `appsettings.json` files and replace the JWT signing key with a strong local secret.
3. Create databases using `database/create-databases.sql` or EF migrations.
4. Run the Event API first because Ticket API calls it through `IEventCatalogClient`.
5. Run tests with:

```powershell
dotnet test .\Ripple.Ticketing.sln
```

## Running locally

```powershell
dotnet run --project .\src\EventManagement\Ripple.EventManagement.Api\Ripple.EventManagement.Api.csproj
dotnet run --project .\src\TicketManagement\Ripple.TicketManagement.Api\Ripple.TicketManagement.Api.csproj
```

OpenAPI documents are available at `/openapi/v1.json` because each API calls `app.MapOpenApi()`.

## Security design

- SQL injection is mitigated by EF Core parameterized queries and no string-concatenated SQL.
- JWT bearer auth validates issuer, audience, expiry, and signing key.
- Roles are enforced through named policies such as `CanManageEvents`, `CanBuyTickets`, and `CanViewReports`.
- CORS allows only configured trusted client origins.
- Rate limiting uses a fixed-window limiter.
- Request body size is limited through Kestrel and form options.
- Errors are centralized in `ApiExceptionFilter`; production responses hide stack traces and sensitive details.
- Logging is centralized through Serilog console and rolling file sinks.

## Overselling prevention

Ticket inventory is represented by `TicketInventory`. The domain method blocks purchases that exceed capacity. The EF model has a SQL Server `rowversion` concurrency token. In a production hardening pass, wrap purchase handling in a retry strategy for `DbUpdateConcurrencyException` and consider a stored procedure or serializable transaction for very high write volume.

## AI tooling note

AI was used to accelerate solution scaffolding, identify boilerplate patterns, draft validators/tests/README content, and compare modern .NET APIs. Final architecture choices, tradeoffs, and security posture should still be reviewed before submission.

## Generate a local JWT token

```powershell
dotnet run --project .\tools\TokenGenerator\TokenGenerator.csproj Admin
```

Pass any custom role name, for example `EventManager`, `TicketAgent`, or `Customer`.
