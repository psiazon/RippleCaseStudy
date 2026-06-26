# Ripple Event Ticketing System -- Case Study

## Overview

This solution implements a simplified event ticketing platform using
**ASP.NET Core Web API** and **Clean Architecture**. The system is split
into two independently deployable microservices:

-   **Event Management Microservice**
-   **Ticket Management Microservice**

The Ticket service communicates with the Event service through HTTP
APIs.

## Architecture

    Client
       |
       +--> Event Management API ------------------> SQL Server Express (Event DB)
       |             |
       |             +----HTTP----> Ticket Management API
       |
       +--> Ticket Management API ---------------> SQL Server Express (Ticket DB)
                     |
                     +----HTTP----> Event Management API

## Technologies

-   .NET 10
-   ASP.NET Core Web API
-   Clean Architecture
-   CQRS + MediatR
-   Entity Framework Core
-   SQL Server Express
-   FluentValidation
-   JWT Authentication
-   Role/Policy Authorization
-   Rate Limiting
-   OpenAPI / Swagger
-   Serilog (logging)
-   xUnit
-   SpecFlow (API automation)
-   React (optional UI)

## Event Management API

### Endpoints

  Method   Endpoint           Description
  -------- ------------------ -----------------
  GET      /api/events        Get all events
  GET      /api/events/{id}   Get event by Id
  POST     /api/events        Create event
  PUT      /api/events/{id}   Update event
  DELETE   /api/events/{id}   Delete event

### Create Event Flow

1.  Validate request.
2.  Save Event.
3.  Commit transaction.
4.  Call Ticket API `POST /api/tickets/inventory`.
5.  Ticket API creates inventory.
6.  Return created Event.

## Ticket Management API

### Endpoints

  Method   Endpoint                              Description
  -------- ------------------------------------- --------------------
  POST     /api/tickets/purchase                 Purchase ticket
  POST     /api/tickets/inventory                Create inventory
  GET      /api/tickets/availability/{eventId}   Check availability
  GET      /api/tickets/sales-report/{eventId}   Sales report

### Purchase Flow

1.  Validate request.
2.  Call Event API.
3.  Verify event and capacity.
4.  Update inventory.
5.  Create sales record.
6.  Return confirmation.

## Security

-   JWT Bearer Authentication
-   Role-based Authorization
-   Policy Authorization
-   FluentValidation
-   SQL Injection protection (EF Core)
-   Rate limiting
-   CORS
-   Global exception handling

## Testing

-   xUnit unit tests
-   SpecFlow API tests
-   Code coverage target: 90%

## Running

### APIs

``` bash
dotnet restore
dotnet build
dotnet run
```

### React UI

``` bash
npm install
npm run dev
```

## Improvements

-   Distributed cache (Redis)
-   Event-driven messaging
-   Docker / Kubernetes
-   CI/CD
-   OpenTelemetry

## Author

Prepared as a Ripple Senior Software Backend Engineer technical case
study.
