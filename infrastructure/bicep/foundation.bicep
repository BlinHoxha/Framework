targetScope = 'resourceGroup'

@description('Lowercase letters and digits; use a globally unique prefix for DNS-named services.')
@minLength(5)
@maxLength(15)
param prefix string
param location string = resourceGroup().location
param tags object = {}
@secure()
param sqlAdministratorPassword string
param sqlAdministratorLogin string = 'frameworkadmin'
param chatModel string = 'gpt-4o-mini'
param chatModelVersion string
param embeddingModel string = 'text-embedding-3-small'
param embeddingModelVersion string
param chatCapacity int = 10
param embeddingCapacity int = 10
@description('Development only: permit connections from Azure services to SQL. Use private networking for production.')
param allowAzureServicesToSql bool = false

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: '${prefix}acr'
  location: location
  sku: { name: 'Basic' }
  properties: { adminUserEnabled: false }
  tags: tags
}

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-logs'
  location: location
  properties: { retentionInDays: 30, sku: { name: 'PerGB2018' } }
  tags: tags
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${prefix}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
  tags: tags
}

resource openAi 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: '${prefix}-openai'
  location: location
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: {
    customSubDomainName: '${prefix}-openai'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }
  tags: tags
}

resource chat 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAi
  name: 'chat'
  sku: { name: 'Standard', capacity: chatCapacity }
  properties: { model: { format: 'OpenAI', name: chatModel, version: chatModelVersion } }
}

resource embeddings 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAi
  name: 'embeddings'
  sku: { name: 'Standard', capacity: embeddingCapacity }
  properties: { model: { format: 'OpenAI', name: embeddingModel, version: embeddingModelVersion } }
}

resource search 'Microsoft.Search/searchServices@2025-05-01' = {
  name: '${prefix}-search'
  location: location
  sku: { name: 'basic' }
  properties: {
    disableLocalAuth: true
    authOptions: { aadOrApiKey: { aadAuthFailureMode: 'http401WithBearerChallenge' } }
    publicNetworkAccess: 'Enabled'
    replicaCount: 1
    partitionCount: 1
  }
  tags: tags
}

resource sql 'Microsoft.Sql/servers@2023-08-01' = {
  name: '${prefix}-sql'
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    publicNetworkAccess: 'Enabled'
    minimalTlsVersion: '1.2'
  }
  tags: tags
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sql
  name: 'framework'
  location: location
  sku: { name: 'Basic', tier: 'Basic' }
  tags: tags
}

resource azureServicesFirewall 'Microsoft.Sql/servers/firewallRules@2023-08-01' = if (allowAzureServicesToSql) {
  parent: sql
  name: 'AllowAzureServices'
  properties: { startIpAddress: '0.0.0.0', endIpAddress: '0.0.0.0' }
}

output registryName string = registry.name
output containerEnvironmentName string = environment.name
output openAiEndpoint string = 'https://${openAi.name}.openai.azure.com/'
output searchEndpoint string = 'https://${search.name}.search.windows.net/'
output searchName string = search.name
output sqlServerName string = sql.name
output sqlDatabaseName string = database.name
