# General Azure Bicep

Deploy resource-group.bicep at subscription scope, then foundation.bicep at resource-group scope. Supply prefix, location/tags and secure SQL administrator credentials. The foundation creates registry, logs, Container Apps environment and Azure SQL; it accepts no AI model parameters.

Push framework-api:<tag>, then deploy app.bicep with prefix, imageTag, authority, audience and secure sqlConnectionString. The identity receives AcrPull only; review production SQL access separately.

AI modules moved to AI-Framework/infrastructure/azure/bicep. Review ownership/state migration in the parent README before applying changes to existing deployments.