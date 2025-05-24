using Microsoft.Extensions.DependencyInjection;
using PollingStationAPI.Service.Services;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service;

public static class Installer
{
    public static void AddAPIServices(this IServiceCollection services)
    {
        services.AddScoped<IPollingStationService, PollingStationService>();
        services.AddScoped<ICommitteeMemberService, CommitteeMemberService>();
    }
}
