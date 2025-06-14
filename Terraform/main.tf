terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.23.0"
    }

    http = {
      source  = "hashicorp/http"
      version = "2.2.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.3.2"
    }
  }
  required_version = ">= 1.1.0"
  backend "azurerm" {}
}

provider "azurerm" {
  storage_use_azuread = true
  features {}
}

provider "http" {}
provider "random" {}
provider "azuread" {}

data "azurerm_client_config" "current" {}

# Create a new resource group for the eVoting Project
resource "azurerm_resource_group" "eVoting_rg" {
  name     = var.eVoting_rg_name
  location = var.rg_location
}

# Create the Function Apps
module "function" {
  source = "./modules/function"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
    module.vnet
  ]
  func_asp_name        = var.func_asp_name
  func_st_name         = var.func_st_name
  resource_group       = var.eVoting_rg_name
  voting_func_name     = var.voting_func_name
  voting_func_snet_id  = module.vnet.voting_func_snet_id
  voting_func_pep_name = var.voting_func_pep_name
  websites_dns_id      = module.vnet.websites_dns_id
  application_insights_connection = module.azure-monitor.application_insights_connection
  pep_snet_id  = module.vnet.pep_snet_id
}

#Create the Web Apps
module "web-app" {
  source = "./modules/web-app"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
    module.vnet,
    module.function
  ]

  app_asp_name                    = var.app_asp_name
  electoral_register_app_name     = var.electoral_register_app_name
  resource_group                  = var.eVoting_rg_name
  polling_station_app_name        = var.polling_station_app_name
  polling_station_api_name        = var.polling_station_api_name
  voting_app_name                 = var.voting_app_name
  voting_func_name                = var.voting_func_name
  polling_station_api_snet_id     = module.vnet.polling_station_api_snet_id
  portal_apps_snet_id             = module.vnet.portal_apps_snet_id
  application_insights_connection = module.azure-monitor.application_insights_connection
  pep_snet_id                     = module.vnet.pep_snet_id
  polling_station_api_pep_name    = var.polling_station_api_pep_name
  websites_dns_id                 = module.vnet.websites_dns_id
}

module "database" {
  source = "./modules/cosmos-db"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
    module.vnet
  ]
  cosmos_acc_name = var.cosmos_acc_name
  resource_group  = var.eVoting_rg_name
  cosmos_dns_id   = module.vnet.cosmos_dns_id
  cosmos_pep_name = var.cosmos_pep_name
  pep_snet_id     = module.vnet.pep_snet_id
}

module "storage-account" {
  source = "./modules/storage-account"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
    module.vnet
  ]
  resource_group         = var.eVoting_rg_name
  votes_st_name          = var.voting_st_name
  st_blob_dns_id         = module.vnet.st_blob_dns_id
  votes_st_blob_pep_name = var.votes_st_blob_pep_name
  voting_st_snet_id      = module.vnet.voting_st_snet_id
}

module "vnet" {
  source = "./modules/vnet"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
  ]

  cosmos_db_snet_name           = var.cosmos_db_snet_name
  portal_apps_snet_name         = var.portal_apps_snet_name
  polling_station_api_snet_name = var.polling_station_api_snet_name
  resource_group                = var.eVoting_rg_name
  vnet_name                     = var.vnet_name
  voting_st_snet_name           = var.voting_st_snet_name
  voting_func_snet_name         = var.voting_func_snet_name
  private_endpoints_snet_name   = var.private_endpoints_snet_name
}

module "role-assignments" {
  source = "./modules/role-assignments"
  depends_on = [
    azurerm_resource_group.eVoting_rg
  ]

  polling_station_api_principal_id = module.web-app.polling_station_api_principal_id
  votes_st_id                      = module.storage-account.votes_st_id
  voting_func_principal_id         = module.function.voting_func_principal_id
}

module "azure-monitor" {
  source = "./modules/azure-Monitor"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
    module.vnet
  ]
  resource_group            = var.eVoting_rg_name
  application_insights_name = var.application_insights_name
}