resource "azurerm_signalr_service" "signalr_service" {
  name                = var.signalr_name
  location            = var.location
  resource_group_name = var.resource_group

  sku {
    name     = "Free_F1"
    capacity = 1
  }

  #cors {
  #  allowed_origins = [
  #    "https://${var.electoral_register_app_name}.azurewebsites.net",
  #    "https://${var.polling_station_app_name}.azurewebsites.net",
  #    "https://localhost:7137",
  #    "https://localhost:6000"
  #  ]
  #}

  public_network_access_enabled = true

  connectivity_logs_enabled = true
  messaging_logs_enabled    = true
  service_mode = "Serverless"

  #upstream_endpoint {
  #  category_pattern = ["messages"]
  #  event_pattern    = ["*"]
  #  hub_pattern      = ["votingHub"]
  #  url_template     = "https://${var.voting_func_name}.azurewebsites.net"
  #}
}
