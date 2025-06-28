using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using PollingStationAPI.Data;
using PollingStationAPI.Service;
using PollingStationAPI.VotingHub;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile($"appsettings.json", false, true);

builder.Services.AddControllers();

#if !DEBUG
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddApplicationInsights();
#endif

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin() 
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Description = "Adds token to header",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "bearer",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });
});

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(5); // Waits max 10s for a ping
    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton<IUserIdProvider, OidUserIdProvider>();

builder.Services.AddCosmosDb(builder.Configuration);
builder.Services.AddAPIServices();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
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
