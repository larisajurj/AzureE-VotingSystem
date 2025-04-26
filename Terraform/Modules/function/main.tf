# Create a Storage Account Functions Metadata & Keys
resource "azurerm_storage_account" "func_st" {
  name                            = var.func_st_name
  resource_group_name             = var.resource_group
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
}

## Create a Storage Account for the Voting Function
#resource "azurerm_storage_account" "voting_func_st" {
#  name                            = var.voting_func_st_name
#  resource_group_name             = var.resource_group
#  location                        = var.location
#  account_tier                    = "Standard"
#  account_replication_type        = "LRS"
#  public_network_access_enabled   = false
#  allow_nested_items_to_be_public = false
#  shared_access_key_enabled       = false
#}

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
  
  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }
  
  #app_settings = {
  #  "FUNCTIONS_WORKER_RUNTIME"    = "dotnet-isolated"
  #  "FUNCTIONS_EXTENSION_VERSION" = "~4"
# #   "WEBSITE_RUN_FROM_PACKAGE"    = "1" # if you deploy zip package
  #}
  depends_on = [
    azurerm_service_plan.func_asp,
    azurerm_storage_account.func_st
  ]
}
