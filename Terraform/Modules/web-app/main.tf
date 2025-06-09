#Service Plan Resource
resource "azurerm_service_plan" "web_app_service_plan" {
  name                = var.app_asp_name
  resource_group_name = var.resource_group
  location            = var.location
  sku_name            = "B1"
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
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"    = "true"
    "WEBSITE_RUN_FROM_PACKAGE"           = "1"
  }

  site_config {
    always_on = false

    application_stack {
      dotnet_version = "9.0"
    }
  }

  virtual_network_subnet_id = var.portal_apps_snet_id

}


#Polling Station API Web App Resource
resource "azurerm_linux_web_app" "polling_station_api" {
  name                = var.polling_station_api_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    APPLICATIONINSIGHTS_CONNECTION_STRING = var.application_insights_connection
  }

  site_config {
    always_on = false

    application_stack {
      dotnet_version = "9.0"
    }

  }

  virtual_network_subnet_id = var.polling_station_api_snet_id

}

#Polling Station Portal Web App Resource
resource "azurerm_linux_web_app" "polling_station_app" {
  name                = var.polling_station_app_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  app_settings = {
    ConnectionStrings__PollingStationAPI                = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    ClientConfigurations__PollingStationClient__BaseURL = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    DetailedErrors                                      = true
  }

  site_config {
    always_on = false

    application_stack {
      dotnet_version = "9.0"
    }
  }

  virtual_network_subnet_id = var.portal_apps_snet_id

}


#Voting Web App Resource
resource "azurerm_linux_web_app" "voting_app" {
  name                = var.voting_app_name
  resource_group_name = var.resource_group
  location            = var.location
  service_plan_id     = azurerm_service_plan.web_app_service_plan.id

  app_settings = {
    ClientConfigurations__PollingStationClient__BaseURL = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    ConnectionStrings__PollingStationAPI                = "https://${lower(azurerm_linux_web_app.polling_station_api.name)}.azurewebsites.net"
    ClientConfigurations__VotingFunction__BaseURL       = "https://${lower(var.voting_func_name)}.azurewebsites.net"
    DetailedErrors                                      = true
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                   = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                          = "1"
  }

  site_config {
    always_on = false

    application_stack {
      dotnet_version = "9.0"
    }
  }

  virtual_network_subnet_id = var.portal_apps_snet_id

}
