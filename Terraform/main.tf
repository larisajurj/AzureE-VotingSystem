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
  ]
  func_asp_name           = var.func_asp_name
  func_st_name            = var.func_st_name
  resource_group          = var.eVoting_rg_name
  voting_func_name        = var.voting_func_name
}

#Create the Web Apps
module "web-app" {
  source = "./modules/web-app"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
  ]

  app_asp_name                = var.app_asp_name
  electoral_register_app_name = var.electoral_register_app_name
  resource_group              = var.eVoting_rg_name
  polling_station_app_name    = var.polling_station_app_name
  polling_station_api_name    = var.polling_station_api_name
  voting_app_name             = var.voting_app_name
  voting_func_name            = var.voting_func_name
}

module "database" {
  source = "./modules/cosmos-db"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
  ]
  cosmos_acc_name = var.cosmos_acc_name
  resource_group  = var.eVoting_rg_name
}

module "storage-account" {
  source = "./modules/storage-account"
  depends_on = [
    azurerm_resource_group.eVoting_rg,
  ]
  resource_group = var.eVoting_rg_name
  votes_st_name  = var.voting_st_name
}

#module "vnet" {
#  source = "./modules/vnet"
#  depends_on = [
#    azurerm_resource_group.eVoting_rg,
#  ]
#
#  func_st_snet_name              = var.func_st_snet_name
#  polling_station_app_snet_name  = var.polling_station_app_snet_name
#  polling_station_func_snet_name = var.polling_station_func_snet_name
#  resource_group                 = var.eVoting_rg_name
#  vnet_name                      = var.vnet_name
#  voting_app_snet_name           = var.voting_app_snet_name
#  voting_func_snet_name          = var.voting_func_snet_name
#  voting_st_snet_name            = var.voting_st_snet_name
#}