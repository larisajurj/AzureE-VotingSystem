#Resource Group
variable "eVoting_rg_name" {
  type = string
}
variable "rg_location" {
  default = "westeurope"
}

#Functions Service Plan & Storage Account
variable "func_asp_name" {
  type = string
}

variable "func_st_name" {
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

#Polling Station Fn

#Polling Station Db

#Voting App

#Voting Fn
variable "voting_func_name" {
  type = string
}