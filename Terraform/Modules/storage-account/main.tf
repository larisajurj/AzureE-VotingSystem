resource "azurerm_storage_account" "votes_st" {
  name                     = var.votes_st_name
  resource_group_name      = var.resource_group
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "BlobStorage"
}

resource "azurerm_storage_container" "votes_container" {
  name                  = "votes-container"
  storage_account_id    = azurerm_storage_account.votes_st.id
  container_access_type = "private"
}