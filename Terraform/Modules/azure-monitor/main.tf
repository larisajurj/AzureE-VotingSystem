# Create an Application Insights instance
resource "azurerm_log_analytics_workspace" "log_analytics_workspace" {
  name                = "voring-workspace"
  location            = var.location
  resource_group_name = var.resource_group
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "application_insights" {
  name                          = var.application_insights_name
  location                      = var.location
  resource_group_name           = var.resource_group
  workspace_id                  = azurerm_log_analytics_workspace.log_analytics_workspace.id
  application_type              = "web"
}

