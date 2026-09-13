using MachineServiceLab.Api.Contracts;
using MachineServiceLab.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MachineServiceLab.Api.Controllers;

[ApiController]
[Route("api/machines")]
public sealed class MachinesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MachineResponse>> Register(
        RegisterMachineRequest request,
        CancellationToken cancellationToken)
    {
        var machine = await db.Machines.FindAsync(
            [request.SerialNumber],
            cancellationToken);

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

        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(machine));
    }

    [HttpGet("{serialNumber}")]
    public async Task<ActionResult<MachineResponse>> Get(
        string serialNumber,
        CancellationToken cancellationToken)
    {
        var machine = await db.Machines
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.SerialNumber == serialNumber,
                cancellationToken);

        return machine is null
            ? NotFound()
            : Ok(ToResponse(machine));
    }

    private static MachineResponse ToResponse(MachineEntity machine) =>
        new(
            machine.SerialNumber,
            machine.Model,
            machine.FirmwareVersion,
            machine.RegisteredAt);
}