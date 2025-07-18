resource "azurerm_cosmosdb_account" "cosmos_acc" {
  name                = var.cosmos_acc_name
  location            = var.location
  resource_group_name = var.resource_group
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  capabilities {
    name = "EnableServerless"
  }

  consistency_policy {
    consistency_level = "Session"
  }
  
  identity {
    type = "SystemAssigned"
  }
  
  geo_location {
    location          = var.location
    failover_priority = 0
  }

    ip_range_filter = [
      # Accept connections from within public Azure datacenters
      "0.0.0.0",

      # Allow access from the Azure portal
      "4.210.172.107",
      "13.88.56.148",
      "13.91.105.215",
      "40.91.218.243"
    ]

    network_acl_bypass_for_azure_services = true
    public_network_access_enabled         = false

}

resource "azurerm_cosmosdb_sql_database" "cosmos_sql_database" {
  name                = "VotingDatabase"
  resource_group_name = var.resource_group
  account_name        = azurerm_cosmosdb_account.cosmos_acc.name
}

# Stores information about each polling station
resource "azurerm_cosmosdb_sql_container" "cosmos_polling_station_sql_container" {
  name                  = "PollingStation"
  resource_group_name   = var.resource_group
  account_name          = azurerm_cosmosdb_account.cosmos_acc.name
  database_name         = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_paths   = ["/id"]
}

# Store information about committee members
resource "azurerm_cosmosdb_sql_container" "cosmos_committee_member_sql_container" {
  name                  = "CommitteeMember"
  resource_group_name   = var.resource_group
  account_name          = azurerm_cosmosdb_account.cosmos_acc.name
  database_name         = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_paths   = ["/id"]
}

# Store information about voting presence
resource "azurerm_cosmosdb_sql_container" "cosmos_summary_sql_container" {
  name                  = "Presence"
  resource_group_name   = var.resource_group
  account_name          = azurerm_cosmosdb_account.cosmos_acc.name
  database_name         = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_paths   = ["/id"]
}

# Store information about voting decisions
resource "azurerm_cosmosdb_sql_container" "cosmos_candidate_sql_container" {
  name                  = "Candidate"
  resource_group_name   = var.resource_group
  account_name          = azurerm_cosmosdb_account.cosmos_acc.name
  database_name         = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_paths   = ["/id"]
}

# Store information about the permanent election register of a station
resource "azurerm_cosmosdb_sql_container" "cosmos_electoral_reg_sql_container" {
  name                  = "ElectoralRegister"
  resource_group_name   = var.resource_group
  account_name          = azurerm_cosmosdb_account.cosmos_acc.name
  database_name         = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_paths   = ["/id"]
}

# Store information about the special election register of a station
resource "azurerm_cosmosdb_sql_container" "cosmos_special_reg_sql_container" {
  name                  = "VoteRecord"
  resource_group_name   = var.resource_group
  account_name          = azurerm_cosmosdb_account.cosmos_acc.name
  database_name         = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_paths   = ["/id"]
}

resource "azurerm_private_endpoint" "cosmos_pep" {
  name                = var.cosmos_pep_name
  location            = var.location
  resource_group_name = var.resource_group
  subnet_id           = var.pep_snet_id

  private_service_connection {
    name                           = "CosmosPrivateLinkConnection"
    private_connection_resource_id = azurerm_cosmosdb_account.cosmos_acc.id
    subresource_names              = ["SQL"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "cosmos_dns_group"
    private_dns_zone_ids = [var.cosmos_dns_id]
  }
}

## Custom role definition for read access to the Cosmos DB
#resource "azurerm_cosmosdb_sql_role_definition" "read_role" {
#  name                = "Read Role"
#  resource_group_name = var.resource_group
#  account_name        = azurerm_cosmosdb_account.cosmos_acc.name
#  type                = "CustomRole"
#  assignable_scopes   = [azurerm_cosmosdb_account.cosmos_acc.id]
#
#  permissions {
#    data_actions = [
#      "Microsoft.DocumentDB/databaseAccounts/readMetadata",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/read",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/executeQuery"
#    ]
#  }
#}
#
## Custom role definition for write access to the Cosmos DB
#resource "azurerm_cosmosdb_sql_role_definition" "write_in_container_role" {
#  name                = "Claim Func Write Role"
#  resource_group_name = var.resource_group
#  account_name        = azurerm_cosmosdb_account.cosmos_acc.name
#  type                = "CustomRole"
#  assignable_scopes = [
#    "${azurerm_cosmosdb_account.cosmos_acc.id}/dbs/${azurerm_cosmosdb_sql_database.cosmos_claims_sql_container.name}/colls/${azurerm_cosmosdb_sql_container.cosmos_rating_sql_container.name}"
#  ]
#
#  permissions {
#    data_actions = [
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/create",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/replace",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/delete",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/read",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/executeQuery",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/upsert"
#    ]
#  }
#}
#
## Custom role definition for write access to the Cosmos DB
#resource "azurerm_cosmosdb_sql_role_definition" "read_write_role" {
#  name                = "ML Func Role"
#  resource_group_name = var.resource_group
#  account_name        = azurerm_cosmosdb_account.cosmos_acc.name
#  type                = "CustomRole"
#  assignable_scopes   = [azurerm_cosmosdb_account.cosmos_acc.id]
#
#  permissions {
#    data_actions = [
#      "Microsoft.DocumentDB/databaseAccounts/readMetadata",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/create",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/replace",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/upsert",
#      "Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/delete"
#    ]
#  }
#}