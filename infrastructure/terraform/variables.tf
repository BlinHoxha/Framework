variable "subscription_id" { type = string }
variable "prefix" {
  type        = string
  description = "Globally unique lowercase letters and digits for DNS-named Azure resources."
  validation {
    condition     = can(regex("^[a-z][a-z0-9]{4,14}$", var.prefix))
    error_message = "Use 5 to 15 lowercase letters or digits, starting with a letter."
  }
}
variable "location" { type = string }
variable "tags" {
  type    = map(string)
  default = {}
}
variable "sql_administrator_login" {
  type    = string
  default = "frameworkadmin"
}
variable "sql_administrator_password" {
  type      = string
  sensitive = true
}






variable "deploy_app" {
  type        = bool
  default     = false
  description = "Enable after the API image exists."
}
variable "image_tag" {
  type    = string
  default = ""
}
variable "authentication_authority" {
  type    = string
  default = ""
}
variable "authentication_audience" {
  type    = string
  default = ""
}
variable "sql_connection_string" {
  type      = string
  sensitive = true
  default   = ""
}
variable "allow_azure_services_to_sql" {
  type        = bool
  default     = false
  description = "Development only. Permits connections from Azure services to SQL; use private networking for production."
}
