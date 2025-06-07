output "azure_voting_vnet_id" {
  value = azurerm_virtual_network.azure_voting_vnet.id
}

output "voting_func_snet_id" {
  value = azurerm_subnet.voting_func_snet.id
}

output "polling_station_api_snet_id" {
  value = azurerm_subnet.polling_station_api_snet.id
}

output "portal_apps_snet_id" {
  value = azurerm_subnet.portal_apps_snet.id
}

output "cosmos_db_snet_id" {
  value = azurerm_subnet.cosmos_db_snet.id
}

output "voting_st_snet_id" {
  value = azurerm_subnet.voting_st_snet.id
}

output "cosmos_dns_id" {
  value = azurerm_private_dns_zone.cosmos_dns.id
}

output "st_blob_dns_id" {
  value = azurerm_private_dns_zone.st_blob_dns.id
}

output "websites_dns_id" {
  value = azurerm_private_dns_zone.websites_dns.id
}
