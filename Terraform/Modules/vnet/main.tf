# Create Vnet
resource "azurerm_virtual_network" "azure_voting_vnet" {
  name                = var.vnet_name
  location            = var.location
  resource_group_name = var.resource_group
  address_space       = ["10.0.0.0/16"]
}

# Create SNET for Voting Func
resource "azurerm_subnet" "voting_func_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name             = var.resource_group
  virtual_network_name            = var.vnet_name
  name                            = var.voting_func_snet_name
  address_prefixes                = ["10.0.3.0/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}

# Create SNET for Polling Station Func
resource "azurerm_subnet" "polling_station_func_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name             = var.resource_group
  virtual_network_name            = var.vnet_name
  name                            = var.polling_station_func_snet_name
  address_prefixes                = ["10.0.3.32/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}

# Create SNET for Voting App
resource "azurerm_subnet" "voting_app_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name             = var.resource_group
  virtual_network_name            = var.vnet_name
  name                            = var.voting_app_snet_name
  address_prefixes                = ["10.0.3.64/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}

# Create SNET for Voting Func
resource "azurerm_subnet" "polling_station_app_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name             = var.resource_group
  virtual_network_name            = var.vnet_name
  name                            = var.polling_station_app_snet_name
  address_prefixes                = ["10.0.3.128/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}

# Create SNET for Func Storage Account Private Endpoint
resource "azurerm_subnet" "func_st_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name             = var.resource_group
  virtual_network_name            = var.vnet_name
  name                            = var.func_st_snet_name
  address_prefixes                = ["10.0.3.160/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}

# Create SNET for Func Storage Account Private Endpoint
resource "azurerm_subnet" "voting_st_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name             = var.resource_group
  virtual_network_name            = var.vnet_name
  name                            = var.voting_st_snet_name
  address_prefixes                = ["10.0.3.192/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}