#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Signal R
variable "signalr_name" {
  type = string
}

#Web Apps
variable "electoral_register_app_name" {
  type = string
}

variable "polling_station_app_name" {
  type = string
}

#Function App
variable "voting_func_name" {
  type = string
}