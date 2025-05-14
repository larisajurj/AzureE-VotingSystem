using PollingStationAPI.Data;
using PollingStationAPI.Service;
using PollingStationAPI.VotingHub;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile($"appsettings.json", false, true);

builder.Services.AddControllers();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("PollingStationPolicy", builder =>
//    {
//        builder
//            .WithOrigins("https://localhost:7137/",
//            "https://localhost:5072",
//            "https://POLLING-STATION-PORTAL.azurewebsites.net", 
//            "https://VOTING-PORTAL-RO.azurewebsites.net")
//            .AllowAnyMethod()
//            .AllowAnyHeader()
//            .AllowCredentials()
//            .SetIsOriginAllowed(hosts => true);
//    });
//});
//

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()  //  <---  Make sure this is correct for production!
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(5); // Waits max 10s for a ping
    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
});
builder.Services.AddCosmosDb(builder.Configuration);
builder.Services.AddAPIServices();

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        "default",
        "admin/{controller=Home}/{action=Index}/{id?}");
    endpoints.MapHub<VotingHub>("/voting");
});

app.MapControllers();

app.Run();
