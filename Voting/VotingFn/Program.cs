using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VotingFn.Factory.Interface;
using VotingFn.Factory;
using VotingFn.Clients.Interface;
using VotingFn;
using VotingFn.Clients;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
	.ConfigureAppConfiguration(config =>
	{
		config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
		config.AddEnvironmentVariables();
	})
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(services => 
	{
		services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
		services.AddSingleton<IVotingService, VotingService>();
	})
	.Build();

host.Run();