# General Azure Terraform

Configure subscription_id, location, prefix, SQL credentials and a protected remote backend. Start with deploy_app=false, review the plan and provision the foundation. Push the image before enabling the app with image_tag, authentication_authority, authentication_audience and secure sql_connection_string.

This root owns general hosting/database resources only. The AI alternative child module lives in AI-Framework/infrastructure/azure/terraform, composed by application roots.

Existing environments require reviewed AI resource state moves/imports before applying this split. Preserve state/data and reject unintended destroy/recreate actions. No deployment was performed during extraction.