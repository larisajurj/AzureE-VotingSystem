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

variable "func_st_snet_name" {
  type = string
}

#Polling Station Function SNET
variable "polling_station_func_snet_name" {
  type = string
}

#Polling Station App SNET
variable "polling_station_app_snet_name" {
  type = string
}

#Voting Function SNET
variable "voting_func_snet_name" {
  type = string
}

#Voting App SNET
variable "voting_app_snet_name" {
  type = string
}