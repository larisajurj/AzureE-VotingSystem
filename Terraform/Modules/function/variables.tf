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
variable "func_asp_name" {
  type = string
}

#Storage Account
variable "func_st_name" {
  type = string
}

#Voting Function
variable "voting_func_name" {
  type = string
}

variable "voting_func_snet_id" {
  type = string
}

variable "pep_snet_id" {
  type = string
}

variable "voting_func_pep_name" {
  type = string
}

variable "websites_dns_id" {
  type = string
}
