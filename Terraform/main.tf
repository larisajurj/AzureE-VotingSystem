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

  backend "azurerm" {}
  required_version = "1.11.3"
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


