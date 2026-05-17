# Add Service Layer and Unit Tests to CvTracker.Api

## Context

.NET 10 ASP.NET Core Web API. Controllers (`JobApplicationsController`, `CompaniesController`) currently call `AppDbContext` directly. The solution (`CV Tracker.sln`) contains only the API project; `CvTracker.Api.Tests` exists as a directory but is not yet in the solution.

## What needs to be done

### 1. Service layer

Extract all business logic from both controllers into services (`JobApplicationService`, `CompanyService`) with corresponding interfaces. Register them as scoped in `Program.cs`. Controllers must only call the service and return an HTTP response — no direct `AppDbContext` usage in controllers.

### 2. Test project

Add `CvTracker.Api.Tests` to the solution. Set it up as an xUnit project targeting `net10.0` with: **FluentAssertions**, **Moq**, and **EF Core InMemory** provider.

### 3. Unit tests

Write unit tests for both services. Use EF Core InMemory (fresh DB per test) instead of mocking `AppDbContext`. Use FluentAssertions for all assertions. Follow the AAA pattern and name tests using `MethodName_WhenCondition_ShouldExpectedResult`.

Cover happy path, not-found, and empty-collection scenarios for every service method.

## Constraints

- No repository layer — services call `AppDbContext` directly
- Do not modify migrations or add unrelated concerns
- All tests must pass: `dotnet test "CV Tracker.sln"`
