output "voting_func_principal_id" {
  value = azurerm_linux_function_app.voting_func.identity[0].principal_id
}