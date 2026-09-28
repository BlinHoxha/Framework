resource "azurerm_resource_group" "main" {
  name     = "${var.prefix}-rg"
  location = var.location
  tags     = var.tags
}

resource "azurerm_container_registry" "main" {
  name                = "${var.prefix}acr"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = false
  tags                = var.tags
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "${var.prefix}-logs"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_container_app_environment" "main" {
  name                       = "${var.prefix}-env"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  tags                       = var.tags
}

resource "azurerm_cognitive_account" "openai" {
  name                  = "${var.prefix}-openai"
  resource_group_name   = azurerm_resource_group.main.name
  location              = azurerm_resource_group.main.location
  kind                  = "OpenAI"
  sku_name              = "S0"
  custom_subdomain_name = "${var.prefix}-openai"
  local_auth_enabled    = false
  tags                  = var.tags
}

resource "azurerm_cognitive_deployment" "chat" {
  name                 = "chat"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  model {
    format  = "OpenAI"
    name    = var.chat_model
    version = var.chat_model_version
  }
  sku {
    name     = "Standard"
    capacity = var.chat_capacity
  }
}

resource "azurerm_cognitive_deployment" "embeddings" {
  name                 = "embeddings"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  model {
    format  = "OpenAI"
    name    = var.embedding_model
    version = var.embedding_model_version
  }
  sku {
    name     = "Standard"
    capacity = var.embedding_capacity
  }
}

resource "azurerm_search_service" "main" {
  name                         = "${var.prefix}-search"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  sku                          = "basic"
  local_authentication_enabled = false
  tags                         = var.tags
}

resource "azurerm_mssql_server" "main" {
  name                         = "${var.prefix}-sql"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_administrator_login
  administrator_login_password = var.sql_administrator_password
  minimum_tls_version          = "1.2"
  tags                         = var.tags
}

resource "azurerm_mssql_database" "main" {
  name      = "framework"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"
  tags      = var.tags
}

resource "azurerm_mssql_firewall_rule" "azure_services" {
  count            = var.allow_azure_services_to_sql ? 1 : 0
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_user_assigned_identity" "api" {
  name                = "${var.prefix}-api-identity"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  tags                = var.tags
}

resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}

resource "azurerm_role_assignment" "openai_user" {
  scope                = azurerm_cognitive_account.openai.id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}

resource "azurerm_role_assignment" "search_contributor" {
  scope                = azurerm_search_service.main.id
  role_definition_name = "Search Index Data Contributor"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}

resource "azurerm_role_assignment" "search_reader" {
  scope                = azurerm_search_service.main.id
  role_definition_name = "Search Index Data Reader"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}

resource "azurerm_container_app" "api" {
  count                        = var.deploy_app ? 1 : 0
  name                         = "${var.prefix}-api"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  tags                         = var.tags

  lifecycle {
    precondition {
      condition = alltrue([
        var.image_tag != "",
        var.authentication_authority != "",
        var.authentication_audience != "",
        var.sql_connection_string != ""
      ])
      error_message = "image_tag, authentication_authority, authentication_audience, and sql_connection_string are required when deploy_app is true."
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.api.id
  }

  secret {
    name  = "database"
    value = var.sql_connection_string
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = 1
    max_replicas = 3
    container {
      name   = "api"
      image  = "${azurerm_container_registry.main.login_server}/framework-api:${var.image_tag}"
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.api.client_id
      }
      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "database"
      }
      env {
        name  = "Database__Provider"
        value = "SqlServer"
      }
      env {
        name  = "AI__Provider"
        value = "Azure"
      }
      env {
        name  = "AI__Azure__OpenAiEndpoint"
        value = azurerm_cognitive_account.openai.endpoint
      }
      env {
        name  = "AI__Azure__ChatDeployment"
        value = azurerm_cognitive_deployment.chat.name
      }
      env {
        name  = "AI__Azure__EmbeddingDeployment"
        value = azurerm_cognitive_deployment.embeddings.name
      }
      env {
        name  = "AI__Azure__SearchEndpoint"
        value = "https://${azurerm_search_service.main.name}.search.windows.net/"
      }
      env {
        name  = "AI__Azure__SearchIndex"
        value = "knowledge-chunks"
      }
      env {
        name  = "Authentication__Authority"
        value = var.authentication_authority
      }
      env {
        name  = "Authentication__Audience"
        value = var.authentication_audience
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azurerm_role_assignment.openai_user,
    azurerm_role_assignment.search_contributor,
    azurerm_role_assignment.search_reader
  ]
}
