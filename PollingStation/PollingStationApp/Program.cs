using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using PollingStationApp.Components;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Web;
using PollingStationApp.Data.Helpers.Abstractions;
using PollingStationApp.Data.Helpers;
using Microsoft.Identity.Web.UI;
using PollingStationAPI.Data;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using PollingStationApp.Services;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Configuration
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.RoleClaimType = "roles";
    })
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddSignalRSessionServices(builder.Configuration);
builder.Services.AddPollingStationClient(builder.Configuration);
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Required for any static assets

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages();       // Required for identity UI
app.MapControllers();      // Required for /MicrosoftIdentity/Account/SignIn

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
