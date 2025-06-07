output "polling_station_api_principal_id" {
  value = azurerm_linux_web_app.polling_station_api.identity[0].principal_id
}