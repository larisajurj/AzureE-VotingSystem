#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Virtual Network
variable "vnet_name" {
  type = string
}

#Storage Account SNET
variable "voting_st_snet_name" {
  type = string
}

#Cosmos DB SNET
variable "cosmos_db_snet_name" {
  type = string
}

#Polling Station API SNET
variable "polling_station_api_snet_name" {
  type = string
}

variable "private_endpoints_snet_name" {
  type = string
}

#Portal Apps SNET
variable "portal_apps_snet_name" {
  type = string
}

#Voting Function SNET
variable "voting_func_snet_name" {
  type = string
}

