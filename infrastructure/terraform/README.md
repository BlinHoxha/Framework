# Terraform

This root module creates the Azure foundation with `deploy_app=false`. Push the API image and create the search index before setting `deploy_app=true`. Follow the shared deployment steps in [../README.md](../README.md). Configure a protected remote state backend per environment and commit only sanitized variable examples. SQL passwords and the API connection string can be present in Terraform state; restrict state access.
