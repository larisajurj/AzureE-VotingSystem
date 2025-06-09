resource "azurerm_storage_account" "votes_st" {
  name                            = var.votes_st_name
  resource_group_name             = var.resource_group
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  account_kind                    = "BlobStorage"
  public_network_access_enabled   = false
  allow_nested_items_to_be_public = false
}

resource "azurerm_storage_container" "votes_container" {
  name                  = "votes-container"
  storage_account_id    = azurerm_storage_account.votes_st.id
  container_access_type = "private"
}

# Apply Legal Hold Policy to ensure immutability of the votes inside the storage account
resource "azurerm_storage_container_immutability_policy" "legal_hold_policy" {
  storage_container_resource_manager_id = azurerm_storage_container.votes_container.id
  immutability_period_in_days           = 30
  protected_append_writes_all_enabled   = true
  #locked = true  #Whether to lock this immutability policy. Cannot be set to false once the policy has been locked.
}
