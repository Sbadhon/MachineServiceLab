using MachineServiceLab.Api.Contracts;
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
    public async Task<ActionResult<TelemetryResponse>> Create(
        TelemetryRequest request,
        CancellationToken cancellationToken)
    {
        var machineExists = await db.Machines
            .AnyAsync(
                x => x.SerialNumber == request.SerialNumber,
                cancellationToken);

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

        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(telemetry));
    }

    [HttpGet("machines/{serialNumber}/telemetry")]
    public async Task<ActionResult<IReadOnlyList<TelemetryResponse>>> Get(
        string serialNumber,
        CancellationToken cancellationToken)
    {
        var telemetry = await db.Telemetry
            .AsNoTracking()
            .Where(x => x.SerialNumber == serialNumber)
            .OrderByDescending(x => x.Id)
            .Take(20)
            .Select(x => new TelemetryResponse(
                x.Id,
                x.SerialNumber,
                x.Metric,
                x.Value,
                x.Unit,
                x.CapturedAt))
            .ToListAsync(cancellationToken);

        return Ok(telemetry);
    }

    private static TelemetryResponse ToResponse(
        TelemetryEntity telemetry) =>
        new(
            telemetry.Id,
            telemetry.SerialNumber,
            telemetry.Metric,
            telemetry.Value,
            telemetry.Unit,
            telemetry.CapturedAt);
}