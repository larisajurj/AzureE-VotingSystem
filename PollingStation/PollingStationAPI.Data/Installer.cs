using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollingStationAPI.Data.Contexts;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository;
using PollingStationAPI.Data.Repository.Abstractions;
using System.Configuration;

namespace PollingStationAPI.Data;

public static class Installer
{
    public static void AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
	    var connectionString = configuration.GetConnectionString("CosmosDb");
		var databaseName = "VotingDatabase";

        services.AddSingleton(s =>
        {
            var cosmosClient = new CosmosClient(connectionString);
            return cosmosClient;
        });
		 services.AddDbContext<CosmosDbContext>(options =>
			options.UseCosmos(
				connectionString ?? "",
				databaseName
        ));

        services.AddScoped<IRepository<PollingStation, string>>(s =>
        {
            var client = s.GetRequiredService<CosmosClient>();
            return new PollingStationRepository(client);
        });

        services.AddScoped<IRepository<CommitteeMember, string>>(s =>
        {
            var client = s.GetRequiredService<CosmosClient>();
            return new CommitteeMemberRepository(client);
        });

        services.AddScoped<IRepository<RegisteredVoter, Guid>>(s =>
        {
            var client = s.GetRequiredService<CosmosClient>();
            return new ElectoralRegisterRepository(client);
        });

        services.AddScoped<IRepository<VotingRecord, Guid>>(s =>
        {
            var client = s.GetRequiredService<CosmosClient>();
            return new VotingRecordRepository(client);
        });
    }
}
