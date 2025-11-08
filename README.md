# Unilink Task Manager

A simple .NET 9 Web API for managing tasks.

## Features

- CRUD endpoints for tasks (`api/tasks`)
- Filtering by title and completion status
- Sorting by any field (Id, Title, DueDate, IsCompleted) in ascending or descending order
- Pagination support
- Entity Framework Core with in-memory database (no external dependencies)
- DTO validation using data annotations
- NUnit-based test suite with coverage for endpoints and validation
- OpenAPI/Swagger UI for API exploration

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Running the API

1. Restore dependencies:
	```sh
	dotnet restore
	```

2. Build the solution:
	```sh
	dotnet build
	```

3. Run the API:
	```sh
	dotnet run --project TaskManager.Api
	```

4. The API will start on the port specified in `TaskManager.Api/Properties/launchSettings.json` (default: `http://localhost:5121`).

### Accessing Swagger UI

- Open your browser and navigate to:
	- [http://localhost:5121/swagger](http://localhost:5121/swagger)

Swagger UI will display all available endpoints and allow you to interact with the API.

### Running Tests

```sh
dotnet test
```

## API Usage Examples

### Get All Tasks (with default sorting by DueDate ascending)
```http
GET /api/tasks
```

### Get Tasks with Filtering and Sorting
```http
GET /api/tasks?title=meeting&isCompleted=false&sortBy=DueDate&sortOrder=Ascending&page=1&pageSize=10
```

### Sorting Options
- **sortBy**: Field to sort by. Valid values: `Id`, `Title`, `DueDate`, `IsCompleted` (default: `DueDate`)
- **sortOrder**: Sort direction. Valid values: `Ascending`, `Descending` (default: `Ascending`)

## Trade-offs & Improvements

- **Security:** No authentication/authorization implemented for simplicity.
- **Error Handling:** Basic error handling/logging. For production, use structured logging e.g. correlation IDs and consistent error responses.
- **In-Memory Database:** Used for speed and convenience, but not suitable for production. Swap to SQL Server or PostgreSQL for real deployments. When moving to a persistent database, add a code-first migrations project to manage schema changes and database updates using Entity Framework migrations.
- **DTO Validation:** Data annotations are simple but duplicated on both DTOs and entities. Consider using [FluentValidation](https://docs.fluentvalidation.net/en/latest/) for more complex rules e.g. DueDate in the past
- **Tests:** Unit tests cover controller logic directly. For full coverage, add integration tests using [`WebApplicationFactory`](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0).
- **Controller-based API vs Minimal API:** While this app in its current form would be well-suited to a minimal API approach, controllers were chosen to demonstrate a structure that supports future growth, separation of concerns, etc.
- **Single Project Structure:** All code is contained in a single solution for simplicity and ease of demonstration. For larger or production systems, splitting into multiple projects (API, domain, infrastructure, tests) improves maintainability and scalability.
- **Service/Domain Layer:** Due to a lack of business logic, there is no separate service or domain layer. If business rules or complex workflows are introduced, adding such layers would improve maintainability and separation of concerns.
- **Entity Mapping in Repository:** The repository maps entities to DTOs (e.g., TaskResponse) to avoid exposing internal data models directly. This adds a small amount of overhead but improves security, flexibility, and API contract stability.
- **Audit Fields:** No Created/UpdateDate, Created/UpdateBy fields are included in the task entity. Consider adding these fields with automatic population via EF Core's `SaveChanges` override or using interceptors.