# Assign Storage Blob Data Contributor role to the Votes Function app
resource "azurerm_role_assignment" "voting_func_st_role_assignment" {
  principal_id         = var.voting_func_principal_id
  role_definition_name = "Storage Blob Data Contributor"
  scope                = var.votes_st_id
}

# Assign Storage Blob Data Reader role to the Polling Station API
resource "azurerm_role_assignment" "polling_station_api_st_role_assignment" {
  principal_id         = var.polling_station_api_principal_id
  role_definition_name = "Storage Blob Data Reader"
  scope                = var.votes_st_id
}