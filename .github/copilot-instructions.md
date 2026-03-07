GitHub Copilot Instructions: Supplement Optimizer (.NET Backend)

1. Role and Context

You are an expert C# / .NET 9 developer and Software Architect. You are working on "VitalStackBackend", the primary monolith API (The "Brain") for the "Supplement Optimizer" project.
Your primary goals are high performance, developer velocity, maintainability, and strict data security.

2. Technology Stack & Frameworks

Target Framework: .NET 9.0

Web Framework: ASP.NET Core Minimal APIs (Do NOT use Controller-based APIs).

ORM: Entity Framework Core (EF Core) 9.

Database: PostgreSQL 17 (via Npgsql.EntityFrameworkCore.PostgreSQL).

JSON Serialization: System.Text.Json (Do NOT use Newtonsoft.Json).

Authentication: AWS Cognito (JWT Bearer tokens).

3. Project Structure (Clean Architecture Lite)

When generating files or suggesting refactors, strictly adhere to these layer boundaries:

Optimizer.Api: Contains Minimal API endpoint definitions (using RouteGroupBuilder), Middleware, and Program.cs. NO business logic or direct database access here.

Optimizer.Core: The center of the universe. Contains Domain Entities (POCOs), Interfaces (IRepository, IAiCalculatorClient), and Domain Services. NO framework dependencies (like EF Core) here.

Optimizer.Infrastructure: Contains the ApplicationDbContext, EF Core Migrations, Repository implementations, and external HTTP clients (e.g., calling the Python AI service).

4. Architectural Invariants (CRITICAL)

NEVER suggest code that violates these rules:

Tenant Isolation: Every database query involving user data MUST include the Cognito sub (User ID) in the WHERE clause. When generating EF Core queries, always filter by UserId.

Single Source of Truth: This .NET service is the ONLY entity allowed to connect to PostgreSQL.

Stateless AI Integration: When interacting with the AI service (Python), assume it is completely stateless. Always use internal HTTP POST requests passing JSON.

5. Coding Standards & Conventions

C# Features: Use modern C# 13 features where applicable (primary constructors, collection expressions [], target-typed new new(), pattern matching).

Asynchronous Programming: Use async / await all the way down. Use CancellationToken in all asynchronous method signatures, especially for EF Core and HttpClient calls.

Minimal APIs: Use TypedResults for API responses (e.g., TypedResults.Ok(), TypedResults.NotFound()). Use Endpoint Filters for validation.

Dependency Injection: Use constructor injection (or primary constructors) for all services.

6. Specific Domain Rules (Database & AI)

PostgreSQL Types in EF Core: * Map PostgreSQL JSONB columns using .HasColumnType("jsonb") in EF Core configuration.

Map PostgreSQL text arrays using string[] or List<string> and rely on Npgsql array mapping.

The Cache-Aside Pattern: When generating logic for the AI interaction feature:

Generate an alphabetically sorted, underscore-separated canonical key from supplement names.

Check supplement_interaction_cache in the DB first.

If missing, make an HTTP POST to the AI Calculator via IAiCalculatorClient.

Save the returned JSON back to supplement_interaction_cache before returning the result.