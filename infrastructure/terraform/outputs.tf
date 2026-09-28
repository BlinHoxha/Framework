output "resource_group_name" { value = azurerm_resource_group.main.name }
output "registry_name" { value = azurerm_container_registry.main.name }
output "openai_endpoint" { value = azurerm_cognitive_account.openai.endpoint }
output "search_endpoint" { value = "https://${azurerm_search_service.main.name}.search.windows.net/" }
output "sql_server_name" { value = azurerm_mssql_server.main.name }
output "app_url" { value = var.deploy_app ? "https://${azurerm_container_app.api[0].latest_revision_fqdn}" : null }
