using MachineServiceLab.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MachineServiceLab.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class TelemetryController(
    AppDbContext db) : ControllerBase
{
    [HttpPost("telemetry")]
    public async Task<ActionResult<TelemetryEntity>> Create(
        TelemetryRequest request)
    {
        var machineExists =
            await db.Machines.AnyAsync(
                x => x.SerialNumber == request.SerialNumber);

        if (!machineExists)
        {
            return NotFound(new
            {
                message = "Machine is not registered."
            });
        }

        var telemetry = new TelemetryEntity
        {
            SerialNumber = request.SerialNumber,
            Metric = request.Metric,
            Value = request.Value,
            Unit = request.Unit,
            CapturedAt = DateTimeOffset.UtcNow
        };

        db.Telemetry.Add(telemetry);

        await db.SaveChangesAsync();

        return Ok(telemetry);
    }

    [HttpGet("machines/{serialNumber}/telemetry")]
    public async Task<ActionResult<IReadOnlyList<TelemetryEntity>>> Get(
        string serialNumber)
    {
        var telemetry =
            await db.Telemetry
                .Where(x => x.SerialNumber == serialNumber)
                .OrderByDescending(x => x.Id)
                .Take(20)
                .ToListAsync();

        return Ok(telemetry);
    }
}

public sealed record TelemetryRequest(
    string SerialNumber,
    string Metric,
    double Value,
    string Unit);