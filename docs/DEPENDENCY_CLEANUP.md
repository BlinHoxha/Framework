# Dependency cleanup and compatibility

The cleanup separates optional dependencies while retaining existing application APIs. These projects are libraries in the same solution, not additional services or databases.

| Requirement | Select |
| --- | --- |
| Entities and common DTOs | Framework.Domain / Framework.Contracts |
| Existing generic service/repository interfaces without mapping | Framework.Application.Abstractions |
| Application-owned EF context with no database provider selected | Framework.Persistence |
| PostgreSQL | Framework.Persistence.PostgreSql |
| SQL Server | Framework.Persistence.SqlServer |
| AutoMapper-based generic CRUD services | Framework.Mapping.AutoMapper |
| Existing dual-provider host, generic mapped repositories and compatibility | Framework.Infrastructure / Framework.Application |

`Framework.Persistence.FrameworkDbContext` has no mapping/application/provider dependency. It does not discover product configuration or own product migrations. Derive from it and configure the application model explicitly. `AddFrameworkPostgreSql<TContext>(connection)` and `AddFrameworkSqlServer<TContext>(connection)` register only the chosen provider; their optional configure callback supports command timeout, migration assembly and provider-specific options. Connection strings remain application secrets.

ConstructionFlow, BusinessSuite and RadIntel now consume the PostgreSQL adapter with explicit DTO mapping. BusinessSuite also selects the mapping-free Application.Abstractions package. MixPilot retains the compatibility facade because it uses the mapped repositories and generic services. Framework.Web uses the mapping-free contracts.

## Preserved compatibility

Existing public service/repository interfaces, `BaseService`, mapping profile and `AddApplication()` names remain unchanged. The original Framework.Application assembly forwards those types to the lean contracts or optional mapping assembly. Existing constructor signatures and generic constraints remain unchanged. The compatibility EF context keeps its original public name and configuration behavior while deriving from the lean context. Existing `AddInfrastructure(configuration)` continues supporting both database providers.

The legacy source paths for interfaces/services/mapping are linked into exactly one implementation project each; Framework.Application excludes those sources from compilation and contains forwarders. This preserves file history and public assembly resolution without duplicate type definitions.

This is an additive migration. Consumers of old facades still receive their old optional dependencies. Do not interpret optional mapping as remediation of the AutoMapper 12.0.1 advisory: the legacy mapping path still needs a separately validated upgrade or replacement. ConstructionFlow's dependency test ensures its runtime has neither AutoMapper, SQL Server nor the old Infrastructure facade.

## Application and AI ownership

ConstructionFlow's Application layer owns access checks and explicit project, procurement, report and knowledge workflows. Infrastructure implements its `IConstructionStore` queries and atomic save; API write endpoints delegate to workflows and preserve HTTP shapes. Simple read queries remain at the API boundary. Application has no EF or ASP.NET dependency. Domain invariants and EF transactions/concurrency/audit remain in effect.

AI-Framework already exposes provider-neutral ingestion/retrieval/generation contracts. No additional business abstraction was introduced there. ConstructionFlow still owns approval, membership, durable evidence and audit. Provider credentials, deployment, indexing durability and production evaluations remain the consuming product's responsibility.

## Versioned releases

Keep sibling project references during workspace development. New optional packages start at 0.2.0; a coordinated release can override `Version` across the selected Framework libraries. Build and test all consumers before producing a release. Pack all dependencies together and publish only to the selected private feed; no publishing was performed by this cleanup.

```powershell
dotnet restore Framework.sln
dotnet build Framework.sln --no-restore -c Release
dotnet test Framework.sln --no-build --no-restore -c Release
dotnet pack Framework.sln --no-build --no-restore -c Release -p:Version=0.2.0 -o .artifacts/packages
```

Products should switch from project to package references deliberately, pin a tested compatible set and avoid consuming the same assembly by both mechanisms. Compatibility facades can be removed only in a separately announced major release after consumers migrate.

## Runtime migration remains coordinated work

Framework now targets both `net9.0` and `net10.0`. The `net9.0` target retains the existing Microsoft package versions (9.0.8) and Npgsql EF provider (9.0.4). The `net10.0` target uses Microsoft packages 10.0.15 and Npgsql EF provider 10.0.3. AutoMapper and the other packages shared by both targets retain their existing versions. A .NET 10 SDK builds both targets; running the `net9.0` tests also requires a .NET 9 runtime.

In the baseline comparison, the .NET 10 SDK pruned 22 transitive entries from the `net9.0` package graph after multi-targeting; no resolved package versions changed or new packages were added.

Consumers still target `net9.0` and need separate migration tasks. Those tasks should validate product migrations and EF/Npgsql behavior, deployment images and CI SDK settings, and fresh plus upgrade-path PostgreSQL tests. Keep existing package releases available for rollback. .NET 9 support ended on 10 November 2026.

### Open AutoMapper security risk

AutoMapper 12.0.1 remains unchanged. The following high-severity advisories are open and **unmitigated** in Framework and every consumer that uses AutoMapper, including MixPilot:

- [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x): uncontrolled recursion can cause denial of service.
- [GHSA-6vhm-j9h7-f9gh](https://github.com/advisories/GHSA-6vhm-j9h7-f9gh): constructor-parameter mapping bypasses `MaxDepth`.
- [GHSA-gpwp-5797-mjff](https://github.com/advisories/GHSA-gpwp-5797-mjff): other mapping paths bypass `MaxDepth`, including paths without a configuration workaround.

NuGet's NU1903 warnings remain visible. A licensed fixed AutoMapper version or replacement requires a separate owner decision and task (planned as FW-AM-001). This multi-targeting change makes no claim of mitigation.

## Verification evidence

Release checks passed across seven solutions: Framework 19 tests, AI-Framework 23, ConstructionFlow 24, MixPilot 30, BusinessSuite 23, RadIntel-AI 47 and LegalIntelligence 17 (183 total). Framework's compatibility tests resolve forwarded public types through their original assembly names. ConstructionFlow's architecture checks reject EF/HTTP dependencies in Application and unused mapping/SQL Server dependencies in the runtime graph.

ConstructionFlow, BusinessSuite and RadIntel API dependency manifests contain neither AutoMapper, SQL Server nor the legacy Framework.Infrastructure facade. Five new 0.2.0 packages were generated locally with README metadata and inspected; only the explicitly selected mapping package declares AutoMapper. No packages were published.

A fresh isolated PostgreSQL 17 cluster verified ConstructionFlow's two existing migrations, repeatable synthetic seeding, real JWT authentication, project version updates, durable reviewed evidence, retrieval/citations and audit reads. The API and isolated database processes stopped after the test. Existing databases were not used. No new product schema migration was necessary for the dependency/workflow refactor. Cloud deployment and live SQL Server verification were outside these checks.
