using MachineServiceLab.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace MachineServiceLab.Api.Controllers;

[ApiController]
[Route("api/machines")]
public sealed class MachinesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MachineEntity>> Register(
        RegisterMachineRequest request)
    {
        var machine =
            await db.Machines.FindAsync(request.SerialNumber);

        if (machine is null)
        {
            machine = new MachineEntity
            {
                SerialNumber = request.SerialNumber,
                Model = request.Model,
                FirmwareVersion = request.FirmwareVersion,
                RegisteredAt = DateTimeOffset.UtcNow
            };

            db.Machines.Add(machine);
        }
        else
        {
            machine.Model = request.Model;
            machine.FirmwareVersion = request.FirmwareVersion;
        }

        await db.SaveChangesAsync();

        return Ok(machine);
    }

    [HttpGet("{serialNumber}")]
    public async Task<ActionResult<MachineEntity>> Get(
        string serialNumber)
    {
        var machine =
            await db.Machines.FindAsync(serialNumber);

        return machine is null
            ? NotFound()
            : Ok(machine);
    }
}

public sealed record RegisterMachineRequest(
    string SerialNumber,
    string Model,
    string FirmwareVersion);