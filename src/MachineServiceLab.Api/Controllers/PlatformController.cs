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
        var isAzureAppService =
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "WEBSITE_SITE_NAME"));

        var monitoringEnabled =
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "APPLICATIONINSIGHTS_CONNECTION_STRING"));

        return Ok(new
        {
            runtime = ".NET 10",

            hosting = isAzureAppService
                ? "Azure App Service"
                : "Local",

            database = isAzureAppService
                ? "Azure SQL"
                : "SQL Server / Azure SQL",

            monitoring = monitoringEnabled
                ? "Azure Monitor / Application Insights"
                : "Local logging"
        });
    }
}