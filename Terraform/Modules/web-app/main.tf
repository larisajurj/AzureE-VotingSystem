#Service Plan Resource
resource "azurerm_service_plan" "web_app_service_plan" {
  name                = var.app_asp_name
  resource_group_name = var.resource_group
  location            = var.location
  sku_name            = "F1"
  os_type             = "Linux"
}


#Electoral Register Web App Resource
resource "azurerm_linux_web_app" "electoral_register_web_app" {
  name                = var.electoral_register_app_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  app_settings = {
    ConnectionStrings__PollingStationAPI = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
  }

  site_config {
    always_on = false
    
    application_stack  {
      dotnet_version = "9.0"
    }
  }
}


#Polling Station Portal Web App Resource
resource "azurerm_linux_web_app" "polling_station_api" {
  name                = var.polling_station_api_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  app_settings = {
  }

  site_config {
    always_on = false

    application_stack  {
      dotnet_version = "9.0"
    }
    
    
  }
}

#Polling Station API Web App Resource
resource "azurerm_linux_web_app" "polling_station_app" {
  name                = var.polling_station_app_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  app_settings = {
    ConnectionStrings__PollingStationAPI = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    ClientConfigurations__PollingStationClient__BaseURL = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    DetailedErrors = true
  }

  site_config {
    always_on = false

    application_stack  {
      dotnet_version = "9.0"
    }
  }
}


#Voting Web App Resource
resource "azurerm_linux_web_app" "voting_app" {
  name                = var.voting_app_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  app_settings = {
    ClientConfigurations__PollingStationClient__BaseURL = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    ConnectionStrings__PollingStationAPI = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    DetailedErrors = true
  }

  site_config {
    always_on = false

    application_stack  {
      dotnet_version = "9.0"
    }
  }
}
