# Create a Storage Account Functions Metadata
resource "azurerm_storage_account" "func_st" {
  name                            = var.func_st_st_name
  resource_group_name             = var.resource_group
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  public_network_access_enabled   = false
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = false
}

# Create a Storage Account for the Voting Function
resource "azurerm_storage_account" "voting_func_st" {
  name                            = var.voting_func_st_name
  resource_group_name             = var.resource_group
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  public_network_access_enabled   = false
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = false
}

# Create a Service Plan for the Voting Function
resource "azurerm_service_plan" "voting_func_asp" {
  name                = var.voting_func_asp_name
  resource_group_name = var.resource_group
  location            = var.location
  os_type             = "Windows"
  sku_name            = "F1"
}

# Create a Function App for the Voting Function
resource "azurerm_windows_function_app" "func_claim" {
  enabled                       = true
  name                          = var.voting_func_name
  resource_group_name           = var.resource_group
  location                      = var.location
  storage_account_name          = azurerm_storage_account.voting_func_st.name
  service_plan_id               = azurerm_service_plan.voting_func_asp.id
  public_network_access_enabled = true
  storage_uses_managed_identity = true
  virtual_network_subnet_id     = var.voting_func_snet_id

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME                  = "dotnet-isolated"
    APPLICATIONINSIGHTS_AUTHENTICATION_STRING = "Authorization=AAD"
    CosmosOptions__Endpoint                   = var.cosmos_endpoint
    AzureWebJobsStorage__accountName          = azurerm_storage_account.voting_func_st.name
  }

  depends_on = [
    azurerm_service_plan.voting_func_asp,
    azurerm_storage_account.voting_func_st
  ]

  https_only = true
  site_config {
    ftps_state             = "Disabled"
    http2_enabled          = true
    always_on              = false
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
  }

  lifecycle { ignore_changes = [virtual_network_subnet_id] }
}
