using Azure.Monitor.OpenTelemetry.AspNetCore;
using MachineServiceLab.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var azureSqlConnection =
    Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(azureSqlConnection))
    {
        options.UseSqlServer(
            azureSqlConnection,
            sqlOptions => sqlOptions.EnableRetryOnFailure());
    }
    else
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "MachineServiceLab"));
    }
});

var applicationInsightsConnection =
    Environment.GetEnvironmentVariable(
        "APPLICATIONINSIGHTS_CONNECTION_STRING");

if (!string.IsNullOrWhiteSpace(applicationInsightsConnection))
{
    builder.Services
        .AddOpenTelemetry()
        .UseAzureMonitor();
}

var app = builder.Build();

app.MapControllers();

app.Run();