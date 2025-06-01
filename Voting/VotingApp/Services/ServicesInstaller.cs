using BlazorCircuitHandler.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using VotingApp.Factories;
using VotingApp.Services.Abstractions;

namespace VotingApp.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddSignalRSessionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<SignalRService>();
        services.AddScoped<CircuitHandlerService>();
        services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<CircuitHandlerService>());
        services.AddSingleton<IUserOnlineServiceFactory, UserOnlineServiceFactory>();

        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {        
        services.AddHttpClient(name: configuration["ClientConfigurations:PollingStationClient:ClientName"], (client) =>
        {
            client.BaseAddress = new Uri(configuration["ClientConfigurations:PollingStationClient:BaseURL"]);
        });
        services.AddScoped<IPollingStationClient, PollingStationClient>();

        services.AddHttpClient(name: configuration["ClientConfigurations:VotingFunction:ClientName"], (client) =>
        {
            client.BaseAddress = new Uri(configuration["ClientConfigurations:VotingFunction:BaseURL"]);
        });
        services.AddScoped<IVotingClient, VotingClient>();
        return services;


    }

}
