# Ripple.EventManagement.SpecFlowTests

This project contains SpecFlow automation tests for the Event Management Web API.

## What is covered

- Event creation by `EventManager`
- Event listing and lookup by authorized readers
- Anonymous access rejection
- Role-based authorization for event creation
- FluentValidation error handling for invalid create requests
- Event update
- Event delete

## Test runtime

The tests use `WebApplicationFactory<Program>` to run the API in memory. SQL Server is replaced with EF Core InMemory for automation test isolation, and the Ticket Inventory HTTP client is replaced with a fake success handler so event creation tests do not depend on the Ticket Management API.

## Run

From the solution root:

```powershell
dotnet test .\tests\Ripple.EventManagement.SpecFlowTests\Ripple.EventManagement.SpecFlowTests.csproj
```

Or run all tests:

```powershell
dotnet test .\Ripple.EventManagement.sln
```
