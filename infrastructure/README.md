# Infrastructure as code

This directory contains Azure deployment definitions for a resource group, Container Apps, Azure Container Registry, Azure OpenAI, Azure AI Search, and Azure SQL Database. Bicep and Terraform implement the same logical stack. Choose one tool for each environment; their state must not manage the same resources.

## Layout

- `bicep/`: Azure resource group, foundation, and API deployments.
- `terraform/`: equivalent Azure resources with optional API deployment.
- `search-index.json`: Azure AI Search chunk index with a 1536-dimension vector field.
- `create-search-index.ps1`: creates the index using the Azure CLI access token.

Keep reusable modules within the selected tool's directory. Keep environment inputs separate from modules. Use the same logical names for inputs and outputs in both implementations so a consuming solution can choose either tool without changing application code. Provider-specific resource IDs and URLs may differ, but their application configuration purpose should be the same.

## Deployment contract

Both implementations accept a location, globally unique naming prefix, tags, SQL administrator password, and model names, versions, and capacities. The API deployment additionally requires a pushed image, SQL connection string, and identity provider settings. Model availability and capacity vary by Azure region and subscription. The API reads:

| Application setting | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL or Azure SQL connection string, supplied as a deployment secret. |
| `Database__Provider` | `PostgreSql` for local development or `SqlServer` for Azure SQL. |
| `AI__Provider` | `Local` or `Azure`. The Azure adapter requires the five `AI__Azure__*` values. |
| `AI__Azure__OpenAiEndpoint`, `AI__Azure__ChatDeployment`, `AI__Azure__EmbeddingDeployment` | Azure OpenAI endpoint and model deployment names. |
| `AI__Azure__SearchEndpoint`, `AI__Azure__SearchIndex` | Azure AI Search endpoint and index name. |
| `Authentication__Authority`, `Authentication__Audience` | JWT issuer and audience; required outside Development. |

Use .NET's double-underscore environment variable convention for nested settings. Do not put connection strings, credentials, or other secret values in committed parameter files or plain deployment outputs. Avoid passing secrets through Terraform resources and variables where possible: providers may record their values in state even when outputs are marked sensitive. Store Terraform state in a protected remote backend and restrict access to it. Use workload identity or managed identity where the chosen services support it.

## Bootstrap order

1. Select **Bicep or Terraform** for the environment. Set a unique lowercase prefix and confirm that the selected region supports the chosen chat and embedding models and their requested capacity.
2. Deploy the resource group and foundation. Bicep uses `resource-group.bicep` at subscription scope, then `foundation.bicep` at resource group scope. Terraform uses `terraform init` with a protected Azure backend and `terraform apply` with `deploy_app=false`.
3. Build and push `Dockerfile` as `framework-api:<tag>` in the created registry. The deployment identity needs permission to push; the API identity receives `AcrPull` only.
4. Grant the index deployment identity **Search Service Contributor**, **Search Index Data Contributor**, and **Search Index Data Reader** on the search service. Run `create-search-index.ps1 -SearchEndpoint https://<name>.search.windows.net/` after `az login`. The API identity has data-plane roles only; it cannot create or change the index schema.
5. Configure SQL network access and supply a SQL connection string securely. The foundation deliberately creates no broad SQL firewall rule. `allowAzureServicesToSql=true` in Bicep or `allow_azure_services_to_sql=true` in Terraform enables the broad Azure-services firewall rule for development only. Production needs a private network path and a database identity/credential plan.
6. Deploy `app.bicep` with the image tag, JWT authority and audience, and secure SQL connection string, or set `deploy_app=true` and the corresponding Terraform variables. The API expects JWTs containing an application `tenant_id` GUID claim; scoped requests also need a matching `scope_id` GUID claim.

The search schema uses 1536 dimensions, matching the default `text-embedding-3-small` output. If the embedding deployment uses another dimension, change `search-index.json` before creating the index. The index schema and application field names must stay aligned.

## Remaining production work

The Azure adapter supports direct text ingestion, embedding, hybrid retrieval, and grounded chat. Ingestion is still synchronous and not durable: it needs persistent document metadata, a queue/worker or outbox, retry and deletion handling, and operational monitoring before production use. The Azure SQL database is provisioned but the current `FrameworkDbContext` has no document tables or migrations. Azure SQL-to-AI-Search indexing is a separate design choice; this adapter currently pushes chunks directly and does **not** configure an Azure SQL indexer. MCP is also not implemented because no tool contract or trusted MCP clients have been specified.

## Ownership and delivery

Choose **one** implementation to manage a given environment's resource set. Do not deploy Bicep and Terraform against the same resources unless ownership is explicitly divided into separate stacks with separate lifecycle and documented handoff outputs. Changing tools for an existing environment requires an import or migration plan; deploying the second definition directly can duplicate or replace resources.

The application build and tests are independent of the infrastructure tool. A deployment pipeline should validate the chosen templates, review a Bicep what-if or Terraform plan, apply the approved change, inject the application settings, and deploy the API artifact. Configure environment-specific values outside application source control. Resource provisioning and application deployment can be separate pipeline stages.
