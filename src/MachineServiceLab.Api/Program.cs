using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var machines =
    new ConcurrentDictionary<string, MachineRegistration>();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));

app.MapPost("/api/machines", (
    RegisterMachineRequest request) =>
{
    var machine = new MachineRegistration(
        request.SerialNumber,
        request.Model,
        request.FirmwareVersion,
        DateTimeOffset.UtcNow);

    machines[request.SerialNumber] = machine;

    return Results.Ok(machine);
});

app.MapGet("/api/machines/{serialNumber}", (
    string serialNumber) =>
{
    return machines.TryGetValue(serialNumber, out var machine)
        ? Results.Ok(machine)
        : Results.NotFound();
});

app.Run();

public sealed record RegisterMachineRequest(
    string SerialNumber,
    string Model,
    string FirmwareVersion);

public sealed record MachineRegistration(
    string SerialNumber,
    string Model,
    string FirmwareVersion,
    DateTimeOffset RegisteredAt);