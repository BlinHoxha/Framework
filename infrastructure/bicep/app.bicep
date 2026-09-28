targetScope = 'resourceGroup'

@minLength(5)
@maxLength(15)
param prefix string
param imageTag string
param authority string
param audience string
@secure()
param sqlConnectionString string
param location string = resourceGroup().location
param tags object = {}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = { name: '${prefix}acr' }
resource environment 'Microsoft.App/managedEnvironments@2024-03-01' existing = { name: '${prefix}-env' }
resource openAi 'Microsoft.CognitiveServices/accounts@2024-10-01' existing = { name: '${prefix}-openai' }
resource search 'Microsoft.Search/searchServices@2025-05-01' existing = { name: '${prefix}-search' }

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-api-identity'
  location: location
  tags: tags
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, identity.id, 'acr-pull')
  scope: registry
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

resource openAiUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openAi.id, identity.id, 'openai-user')
  scope: openAi
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd')
  }
}

resource searchContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(search.id, identity.id, 'search-data-contributor')
  scope: search
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '8ebe5a00-799e-43f5-93ac-243d3dce84a7')
  }
}

resource searchReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(search.id, identity.id, 'search-data-reader')
  scope: search
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '1407120a-92aa-4202-b7e9-c0e197c71c8f')
  }
}

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${prefix}-api'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: { external: true, targetPort: 8080, transport: 'auto' }
      registries: [{ server: registry.properties.loginServer, identity: identity.id }]
      secrets: [{ name: 'database', value: sqlConnectionString }]
    }
    template: {
      containers: [{
        name: 'api'
        image: '${registry.properties.loginServer}/framework-api:${imageTag}'
        resources: { cpu: json('0.5'), memory: '1Gi' }
        env: [
          { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
          { name: 'AZURE_CLIENT_ID', value: identity.properties.clientId }
          { name: 'ConnectionStrings__DefaultConnection', secretRef: 'database' }
          { name: 'Database__Provider', value: 'SqlServer' }
          { name: 'AI__Provider', value: 'Azure' }
          { name: 'AI__Azure__OpenAiEndpoint', value: 'https://${openAi.name}.openai.azure.com/' }
          { name: 'AI__Azure__ChatDeployment', value: 'chat' }
          { name: 'AI__Azure__EmbeddingDeployment', value: 'embeddings' }
          { name: 'AI__Azure__SearchEndpoint', value: 'https://${search.name}.search.windows.net/' }
          { name: 'AI__Azure__SearchIndex', value: 'knowledge-chunks' }
          { name: 'Authentication__Authority', value: authority }
          { name: 'Authentication__Audience', value: audience }
        ]
        probes: [
          { type: 'Liveness', httpGet: { path: '/health/live', port: 8080 }, periodSeconds: 10 }
          { type: 'Readiness', httpGet: { path: '/health/ready', port: 8080 }, periodSeconds: 10 }
        ]
      }]
      scale: { minReplicas: 1, maxReplicas: 3 }
    }
  }
  tags: tags
  dependsOn: [acrPull, openAiUser, searchContributor, searchReader]
}

output appUrl string = 'https://${app.properties.configuration.ingress.fqdn}'
