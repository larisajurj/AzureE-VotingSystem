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

variable "voting_st_snet_id" {
  type = string
}

variable "st_blob_dns_id" {
  type = string
}

variable "votes_st_blob_pep_name" {
  type = string
}