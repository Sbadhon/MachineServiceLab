using Microsoft.AspNetCore.Mvc;

namespace MachineServiceLab.Api.Controllers;

[ApiController]
public sealed class PlatformController : ControllerBase
{
    [HttpGet("/health")]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy"
        });
    }

    [HttpGet("/api/platform")]
    public ActionResult GetPlatform()
    {
        var azureSqlEnabled =
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "AZURE_SQL_CONNECTIONSTRING"));

        var monitoringEnabled =
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "APPLICATIONINSIGHTS_CONNECTION_STRING"));

        return Ok(new
        {
            runtime = ".NET 10",
            hosting = azureSqlEnabled
                ? "Azure"
                : "Local",
            database = azureSqlEnabled
                ? "Azure SQL"
                : "SQLite",
            monitoring = monitoringEnabled
                ? "Azure Monitor / Application Insights"
                : "Local logging"
        });
    }
}