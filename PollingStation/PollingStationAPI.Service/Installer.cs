using Microsoft.Extensions.DependencyInjection;
using PollingStationAPI.Service.Factories;
using PollingStationAPI.Service.Service;
using PollingStationAPI.Service.Services;
using PollingStationAPI.Service.Services.Abstractions;
using System.Net.Http.Headers;

namespace PollingStationAPI.Service;

public static class Installer
{
    public static void AddAPIServices(this IServiceCollection services)
    {
        services.AddScoped<IPollingStationService, PollingStationService>();
        services.AddScoped<ICommitteeMemberService, CommitteeMemberService>();
        services.AddScoped<IElectoralRegisterService, ElectoralRegisterService>();
        services.AddScoped<IVotingRecordService, VotingRecordService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
        services.AddScoped<IVoteReaderService, VoteReaderService>();

        services.AddHttpClient("VirtualAssistantClient", client =>
         {
             client.Timeout = TimeSpan.FromSeconds(120); 
             client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
         });
        
         services.AddSingleton<IVirtualAssistantService, VirtualAssistantService>();
    }
}
