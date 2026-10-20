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
output sqlServerName string = sql.name
output sqlDatabaseName string = database.name
