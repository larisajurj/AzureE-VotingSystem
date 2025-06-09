#Votes Storage Account
variable "votes_st_id" {
  type = string
}


#Principal IDs for assigning roles
variable "voting_func_principal_id" {
  type = string
}

variable "polling_station_api_principal_id" {
  type = string
}
