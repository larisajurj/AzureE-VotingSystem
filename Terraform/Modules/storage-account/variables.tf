#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Votes Storage Account
variable "votes_st_name" {
  type = string
}

#Principal IDs for assigning roles
variable "voting_func_principal_id" {
  type = string
}

variable "polling_station_api_principal_id" {
  type = string
}
