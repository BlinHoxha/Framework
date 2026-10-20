# Framework

Reusable .NET backend foundations inspired by the NetaFramework pattern: entity and DTO abstractions, optional generic CRUD services/controllers, EF Core persistence, API conventions and general deployment examples. Product rules, resource authorization, entities and migrations belong to consuming applications.

**AI now lives in the independent sibling [AI-Framework](../AI-Framework/README.md) solution.** General Framework libraries, the reference host and infrastructure no longer reference AI libraries or register providers. See [migration guide](docs/AI_EXTRACTION.md).

## Libraries

| Project | Responsibility |
| --- | --- |
| Framework.Domain | Entity interfaces/base types and domain exceptions |
| Framework.Contracts | Common results, entity DTO contracts, pagination and health responses |
| Framework.Application.Abstractions | Provider/mapping-free service and repository contracts |
| Framework.Persistence | Provider-neutral EF context foundation |
| Framework.Persistence.PostgreSql | Opt-in PostgreSQL registration and provider dependency |
| Framework.Persistence.SqlServer | Opt-in SQL Server registration and provider dependency |
| Framework.Mapping.AutoMapper | Optional mapping registration and generic CRUD services |
| Framework.Application | Compatibility facade forwarding existing public contracts/services |
| Framework.Infrastructure | Compatibility context, mapping repositories and existing dual-provider registration |
| Framework.Web | Reusable read-only/CRUD controller bases |
| Framework.Api | Reference host for authentication, errors, Swagger and health; not a shared mandatory service |

Package versions are centralized in Directory.Packages.props. Applications select libraries and own their contexts, migrations and startup. BaseRepository<TEntity,TId,TContext> supports application contexts. PostgreSQL and SQL Server providers are supported; migrations remain provider/application-specific.

For new code, choose the lean persistence provider and use explicit application workflows/mapping. Existing `Framework.Application` and `Framework.Infrastructure` entry points remain compatible for current consumers. See [dependency cleanup and compatibility](docs/DEPENDENCY_CLEANUP.md) for selection, package release and runtime migration guidance.

## Build and run

Use the .NET 9 SDK currently shared by the workspace:

```powershell
dotnet restore Framework.sln
dotnet build Framework.sln --no-restore
dotnet test Framework.sln --no-build --no-restore
dotnet run --project src/Framework.Api --launch-profile http
```

Development Swagger: http://localhost:5280/swagger. Health: /api/v1/health, /health/live, /health/ready and /health. Database readiness requires a configured reachable database. The reference context has no business tables/migrations. Generic controllers are abstract extension points, not concrete business endpoints.

## Configuration

| Environment variable | Purpose |
| --- | --- |
| ConnectionStrings__DefaultConnection | Database secret |
| Database__Provider | PostgreSql or SqlServer |
| Database__CommandTimeoutInSeconds | Default 30 |
| Database__EnableSensitiveDataLogging | Keep disabled outside isolated development |
| Authentication__Authority | JWT issuer; required outside Development |
| Authentication__Audience | API audience; required outside Development |

Development settings contain a localhost PostgreSQL example only. Supply real credentials through secrets/environment configuration. Ignored .env/local settings are not automatically loaded. Outside Development the reference host requires authentication by default; applications own membership/resource authorization.

## Application boundaries

Use generic CRUD where it preserves invariants; use explicit services for approvals, financial posting and DJ planning. Generic repositories do not automatically enforce tenants or permissions. Applications own scoped queries, transaction boundaries, business rules, migrations and authorization, including imports/background jobs.

Provider-separated persistence is available through the new opt-in adapters. Shared security, outbox helpers and observability remain subsequent base improvements; extracting AI does not implement them implicitly.

MixPilot consumes Framework plus AI-Framework. BusinessSuite retains base libraries and an independent AI Core reference for future integration. Historical [MixPilot technical documentation](docs/MIXPILOT_TECHNICAL_GUIDE.md) and its PDF are preserved snapshots; use the migration guide/current product documentation for updated AI paths and ownership.

See [general infrastructure](infrastructure/README.md). NuGet's pre-existing AutoMapper 12.0.1 advisory (GHSA-rvv3-g6hj-g44x) remains unresolved. Coordinate dependency remediation and migration to .NET 10 LTS before .NET 9 support ends on 10 November 2026.
