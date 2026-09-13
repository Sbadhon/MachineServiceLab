using System.Text.Json;
using MachineServiceLab.Api.Contracts;
using MachineServiceLab.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MachineServiceLab.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class DiagnosticsController(
    AppDbContext db) : ControllerBase
{
    [HttpPost("diagnostics")]
    public async Task<ActionResult<DiagnosticsResponse>> Create(
        DiagnosticsRequest request,
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

        var diagnostics = new DiagnosticsEntity
        {
            SerialNumber = request.SerialNumber,
            BatteryPercent = request.BatteryPercent,
            BatteryVoltage = request.BatteryVoltage,
            ControllerTemperatureC =
                request.ControllerTemperatureC,
            MachineHours = request.MachineHours,
            FaultCodesJson =
                JsonSerializer.Serialize(request.FaultCodes),
            CapturedAt = DateTimeOffset.UtcNow
        };

        db.Diagnostics.Add(diagnostics);

        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(diagnostics));
    }

    [HttpGet("machines/{serialNumber}/diagnostics/latest")]
    public async Task<ActionResult<DiagnosticsResponse>> GetLatest(
        string serialNumber,
        CancellationToken cancellationToken)
    {
        var diagnostics = await db.Diagnostics
            .AsNoTracking()
            .Where(x => x.SerialNumber == serialNumber)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return diagnostics is null
            ? NotFound()
            : Ok(ToResponse(diagnostics));
    }

    private static DiagnosticsResponse ToResponse(
        DiagnosticsEntity diagnostics) =>
        new(
            diagnostics.Id,
            diagnostics.SerialNumber,
            diagnostics.BatteryPercent,
            diagnostics.BatteryVoltage,
            diagnostics.ControllerTemperatureC,
            diagnostics.MachineHours,
            JsonSerializer.Deserialize<string[]>(
                diagnostics.FaultCodesJson) ?? [],
            diagnostics.CapturedAt);
}