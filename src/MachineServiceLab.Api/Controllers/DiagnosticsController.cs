using System.Text.Json;
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
    public async Task<ActionResult<DiagnosticsEntity>> Create(
        DiagnosticsRequest request)
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

        await db.SaveChangesAsync();

        return Ok(diagnostics);
    }

    [HttpGet("machines/{serialNumber}/diagnostics/latest")]
    public async Task<ActionResult<DiagnosticsEntity>> GetLatest(
        string serialNumber)
    {
        var diagnostics =
            await db.Diagnostics
                .Where(x => x.SerialNumber == serialNumber)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

        return diagnostics is null
            ? NotFound()
            : Ok(diagnostics);
    }
}

public sealed record DiagnosticsRequest(
    string SerialNumber,
    int BatteryPercent,
    double BatteryVoltage,
    double ControllerTemperatureC,
    double MachineHours,
    string[] FaultCodes);