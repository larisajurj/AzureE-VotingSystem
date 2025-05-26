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
        //services.AddScoped<IUserOnlineService, UserOnlineService>();
        services.AddSingleton<IUserOnlineServiceFactory, UserOnlineServiceFactory>();

        return services;
    }

    public static IServiceCollection AddPollingStationClient(this IServiceCollection services, IConfiguration configuration)
    {
        var x = configuration["ClientConfigurations:PollingStationClient:BaseURL"];
        
        services.AddHttpClient(name: configuration["ClientConfigurations:PollingStationClient:ClientName"], (client) =>
        {
           // client.DefaultRequestHeaders.Add(configuration["ClientConfigurations:PollingStationClient:KeyHeaderName"], configuration["ClientConfigurations:PollingStationClient:Key"]);
            client.BaseAddress = new Uri(configuration["ClientConfigurations:PollingStationClient:BaseURL"]);
        });
        //services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IPollingStationClient, PollingStationClient>();
        return services;


    }

}
