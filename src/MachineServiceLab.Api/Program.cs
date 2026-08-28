using System.Text.Json;
using MachineServiceLab.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("MachineServiceLab")));

var app = builder.Build();

app.MapGet("/health", () =>
    Results.Ok(new { status = "Healthy" }));

app.MapPost("/api/machines", async (
    RegisterMachineRequest request,
    AppDbContext db) =>
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

    return Results.Ok(machine);
});

app.MapGet("/api/machines/{serialNumber}", async (
    string serialNumber,
    AppDbContext db) =>
{
    var machine =
        await db.Machines.FindAsync(serialNumber);

    return machine is null
        ? Results.NotFound()
        : Results.Ok(machine);
});

app.MapPost("/api/diagnostics", async (
    DiagnosticsRequest request,
    AppDbContext db) =>
{
    var machineExists =
        await db.Machines.AnyAsync(
            x => x.SerialNumber == request.SerialNumber);

    if (!machineExists)
    {
        return Results.NotFound(new
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

    return Results.Ok(diagnostics);
});

app.MapGet("/api/machines/{serialNumber}/diagnostics/latest",
    async (
        string serialNumber,
        AppDbContext db) =>
    {
        var diagnostics =
            await db.Diagnostics
                .Where(x => x.SerialNumber == serialNumber)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

        return diagnostics is null
            ? Results.NotFound()
            : Results.Ok(diagnostics);
    });

app.MapPost("/api/telemetry",
    async (
        TelemetryRequest request,
        AppDbContext db) =>
        {
            var machineExists = await db.Machines.AnyAsync(
                x => x.SerialNumber == request.SerialNumber);

            if (!machineExists)
            {
                return Results.NotFound(new
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

            return Results.Ok(telemetry);
        });

app.MapGet("/api/machines/{serialNumber}/telemetry", async (
    string serialNumber,
    AppDbContext db) =>
{
    var telemetry = await db.Telemetry
        .Where(x => x.SerialNumber == serialNumber)
        .OrderByDescending(x => x.Id)
        .Take(20)
        .ToListAsync();

    return Results.Ok(telemetry);
});

app.Run();

public sealed record RegisterMachineRequest(
    string SerialNumber,
    string Model,
    string FirmwareVersion);

public sealed record DiagnosticsRequest(
    string SerialNumber,
    int BatteryPercent,
    double BatteryVoltage,
    double ControllerTemperatureC,
    double MachineHours,
    string[] FaultCodes);

public sealed record TelemetryRequest(
    string SerialNumber,
    string Metric,
    double Value,
    string Unit);