#Resource Group
variable "eVoting_rg_name" {
  type = string
}
variable "rg_location" {
  default = "westeurope"
}

#Virtual Network
variable "vnet_name" {
  type = string
}

#Monitor
variable "application_insights_name" {
  type = string
}

#Functions Service Plan & Storage Account
variable "func_asp_name" {
  type = string
}

variable "func_st_name" {
  type = string
}

variable "cosmos_db_snet_name" {
  type = string
}

#Web App Service Plan
variable "app_asp_name" {
  type = string
}

#RegistrulElectoral Api
variable "electoral_register_app_name" {
  type = string
}

#Polling Station App
variable "polling_station_app_name" {
  type = string
}

variable "portal_apps_snet_name" {
  type = string
}

#Polling Station API
variable "polling_station_api_name" {
  type = string
}

variable "polling_station_api_snet_name" {
  type = string
}

variable "polling_station_api_pep_name" {
  type = string
}
variable "private_endpoints_snet_name" {
  type = string
}
#Voting App
variable "voting_app_name" {
  type = string
}

#Voting Fn
variable "voting_func_name" {
  type = string
}

variable "voting_func_snet_name" {
  type = string
}

variable "voting_func_pep_name" {
  type = string
}

#Voting Storage Account
variable "voting_st_name" {
  type = string
}

variable "voting_st_snet_name" {
  type = string
}

variable "votes_st_blob_pep_name" {
  type = string
}

#CosmosDB
variable "cosmos_acc_name" {
  type = string
}

variable "cosmos_pep_name" {
  type = string
}