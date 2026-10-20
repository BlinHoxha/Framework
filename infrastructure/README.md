# General infrastructure

Azure examples for resource group, registry, logs, Container Apps and Azure SQL. Choose Bicep or Terraform as owner per environment, never both for the same resource set.

AI OpenAI/Search resources, model parameters, roles, index schema and bootstrap script moved to [AI-Framework](../../AI-Framework/infrastructure/README.md). The base host starts without an AI provider. Application deployments compose optional AI modules and settings separately.

## Existing deployments require ownership migration

Do not apply this template split to an existing environment before transferring AI resource ownership. Terraform requires reviewed state moves/imports; applying removed resource definitions without a transfer can destroy/recreate resources. Review Bicep deployment mode and deployment-stack ownership too. Preserve names, identities and indexed data. No cloud state or deployment was changed by extraction.

## Deployment contract

Foundation inputs: location, unique lowercase prefix, tags and secure SQL administrator credentials. Application inputs: pushed framework-api image tag, JWT authority/audience and secure SQL connection string. The API identity receives AcrPull; configure SQL networking/runtime credentials separately.

Application settings: ConnectionStrings__DefaultConnection, Database__Provider=SqlServer, Authentication__Authority and Authentication__Audience. Keep credentials out of committed parameter files and plain outputs. Protect Terraform state, which can contain sensitive values. Prefer managed/workload identities where supported.

## Bootstrap

1. Select one IaC owner and deploy resource group/foundation. Bicep uses resource-group.bicep then foundation.bicep; Terraform starts with deploy_app=false and a protected backend.
2. Build/push framework-api:<tag>. Applications own their schema migrations.
3. Configure SQL network access and runtime connection secret. Broad Azure-services firewall access is development-only; production requires a reviewed network and credential plan.
4. Deploy app.bicep or enable the Terraform app with authentication and connection settings.
5. Verify readiness, access controls, monitoring and backup/restore procedures.

The reference context has no business tables/migrations. BusinessSuite's initial schema is PostgreSQL and cannot use this Azure SQL example unchanged. Base AWS hosting modules remain future work; AI-specific AWS assets belong to AI-Framework.