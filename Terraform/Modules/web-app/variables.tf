#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Application Insights
variable "application_insights_connection" {
  type = string
}


#Service Plan
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

variable "portal_apps_snet_id" {
  type = string
}

#Polling Station Api
variable "polling_station_api_name" {
  type = string
}

variable "pep_snet_id" {
  type = string
}

variable "polling_station_api_pep_name" {
  type = string
}

variable "websites_dns_id" {
  type = string
}

variable "polling_station_api_snet_id" {
  type = string
}

#Voting App
variable "voting_app_name" {
  type = string
}

#Voting Func
variable "voting_func_name" {
  type = string
}
