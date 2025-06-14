# Create Vnet
resource "azurerm_virtual_network" "azure_voting_vnet" {
  name                = var.vnet_name
  location            = var.location
  resource_group_name = var.resource_group
  address_space = ["10.0.0.0/16"]
}

# Create SNET for Voting Func VNET Integration
resource "azurerm_subnet" "voting_func_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name                           = var.resource_group
  virtual_network_name                          = var.vnet_name
  name                                          = var.voting_func_snet_name
  address_prefixes = ["10.0.3.0/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

}

# Create SNET for Polling Station Api VNET integration
resource "azurerm_subnet" "polling_station_api_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name                           = var.resource_group
  virtual_network_name                          = var.vnet_name
  name                                          = var.polling_station_api_snet_name
  address_prefixes = ["10.0.3.32/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}


# Create SNET for Private Endpoints
resource "azurerm_subnet" "pep_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name                           = var.resource_group
  virtual_network_name                          = var.vnet_name
  name                                          = var.private_endpoints_snet_name
  address_prefixes = ["10.0.3.64/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}

# Create SNET for Polling Station App
resource "azurerm_subnet" "portal_apps_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name                           = var.resource_group
  virtual_network_name                          = var.vnet_name
  name                                          = var.portal_apps_snet_name
  address_prefixes = ["10.0.3.128/27"]
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
resource "azurerm_subnet" "voting_st_snet" {
  depends_on = [
    azurerm_virtual_network.azure_voting_vnet
  ]

  resource_group_name                           = var.resource_group
  virtual_network_name                          = var.vnet_name
  name                                          = var.voting_st_snet_name
  address_prefixes = ["10.0.3.192/27"]
  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}

#DNS Zones
resource "azurerm_private_dns_zone" "websites_dns" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = var.resource_group
}

resource "azurerm_private_dns_zone" "cosmos_dns" {
  name                = "privatelink.documents.azure.com"
  resource_group_name = var.resource_group
}

resource "azurerm_private_dns_zone" "st_blob_dns" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.resource_group
}

resource "azurerm_private_dns_zone_virtual_network_link" "websites_dns_vnet_link" {
  name                  = "websites_dns_link"
  resource_group_name   = var.resource_group
  private_dns_zone_name = azurerm_private_dns_zone.websites_dns.name
  virtual_network_id    = azurerm_virtual_network.azure_voting_vnet.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "cosmos_dns_vnet_link" {
  name                  = "cosmos_dns_link"
  resource_group_name   = var.resource_group
  private_dns_zone_name = azurerm_private_dns_zone.cosmos_dns.name
  virtual_network_id    = azurerm_virtual_network.azure_voting_vnet.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "st_blob_dns_vnet_link" {
  name                  = "st_blob_dns_link"
  resource_group_name   = var.resource_group
  private_dns_zone_name = azurerm_private_dns_zone.st_blob_dns.name
  virtual_network_id    = azurerm_virtual_network.azure_voting_vnet.id
}


