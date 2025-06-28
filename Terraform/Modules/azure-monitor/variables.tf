#Resource Group
variable "resource_group" {
  type = string
}
variable "location" {
  default = "westeurope"
}

#Application Insights
variable "application_insights_name" {
  type = string
}
