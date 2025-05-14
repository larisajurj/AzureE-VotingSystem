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