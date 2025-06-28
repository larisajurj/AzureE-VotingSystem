# Create a Storage Account Functions Metadata & Keys
resource "azurerm_storage_account" "func_st" {
  name                            = var.func_st_name
  resource_group_name             = var.resource_group
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
}

# Create a Service Plan for the Voting Function
resource "azurerm_service_plan" "func_asp" {
  name                = var.func_asp_name
  resource_group_name = var.resource_group
  location            = var.location
  os_type             = "Linux"
  sku_name            = "B1"
}

# Create a Function App for the Voting Function
resource "azurerm_linux_function_app" "voting_func" {
  name                          = var.voting_func_name
  resource_group_name           = var.resource_group
  location                      = var.location
  storage_account_name          = azurerm_storage_account.func_st.name
  storage_account_access_key    = azurerm_storage_account.func_st.primary_access_key
  service_plan_id               = azurerm_service_plan.func_asp.id

  identity {
    type = "SystemAssigned"
  }
  
  https_only = true
  site_config {
    ftps_state             = "Disabled"
    http2_enabled          = true
    always_on              = true
    vnet_route_all_enabled = false
    minimum_tls_version    = "1.3"

    ip_restriction_default_action = "Deny"

    ip_restriction {
      service_tag               = "AzureCloud"
      action                    = "Allow"
      priority                  = 101
      name                      = "Allow AzureCloud"
      headers                   = []
      ip_address                = null
      virtual_network_subnet_id = null
    }
    application_stack {
      dotnet_version = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }
  
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"               = "dotnet-isolated"
    "FUNCTIONS_EXTENSION_VERSION"            = "~4"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED" = 1
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"    = "true"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"        = "true"
    "WEBSITE_RUN_FROM_PACKAGE"               = "1"
  }
  
  depends_on = [
    azurerm_service_plan.func_asp,
    azurerm_storage_account.func_st
  ]
  
  virtual_network_subnet_id = var.voting_func_snet_id

}

# Create a Private Endpoint for the Voting Function
resource "azurerm_private_endpoint" "voting_func_pep" {
  name                = var.voting_func_pep_name
  location            = var.location
  resource_group_name = var.resource_group
  subnet_id           = var.pep_snet_id

  private_service_connection {
    name                           = "VotingFunctionAppPrivateLinkConnection"
    private_connection_resource_id = azurerm_linux_function_app.voting_func.id
    subresource_names              = ["sites"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "voting_func_dns_group"
    private_dns_zone_ids = [var.websites_dns_id]
  }
}
