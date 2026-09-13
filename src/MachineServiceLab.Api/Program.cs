using Azure.Monitor.OpenTelemetry.AspNetCore;
using MachineServiceLab.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var sqlConnection = builder.Configuration
    .GetConnectionString("MachineServiceLab")
    ?? throw new InvalidOperationException(
        "Connection string 'MachineServiceLab' is required.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        sqlConnection,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

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