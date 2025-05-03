#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Service Plan 
variable "func_asp_name" {
  type = string
}

#Storage Account
variable "func_st_name" {
  type = string
}

#Polling Station Function
variable "polling_station_fn_name" {
  type = string
}

#Voting Function
variable "voting_func_name" {
  type = string
}