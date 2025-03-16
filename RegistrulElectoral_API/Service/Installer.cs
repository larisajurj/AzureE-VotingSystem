using Microsoft.Extensions.DependencyInjection;
using Service.Services;
using Service.Services.Abstractions;

namespace Service;

public static class Installer
{
	public static void AddServices(this IServiceCollection services)
	{
		services.AddHttpClient();
		services.AddScoped<IWebsiteService, WebsiteService>();

	}
}
