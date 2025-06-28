#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Cosmos DB
variable "cosmos_acc_name" {
  type = string
}

variable "cosmos_pep_name" {
  type = string
}

variable "pep_snet_id" {
  type = string
}

variable "cosmos_dns_id" {
  type = string
}
