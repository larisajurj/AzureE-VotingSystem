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