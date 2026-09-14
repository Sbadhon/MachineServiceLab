using Azure.Monitor.OpenTelemetry.AspNetCore;
using MachineServiceLab.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var sqlConnection =
    builder.Configuration.GetConnectionString("MachineServiceLab")
    ?? builder.Configuration["AZURE_SQL_CONNECTIONSTRING"]
    ?? throw new InvalidOperationException(
        "SQL connection string is required.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        sqlConnection,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

if (!string.IsNullOrWhiteSpace(
        builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services
        .AddOpenTelemetry()
        .UseAzureMonitor();
}

var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();