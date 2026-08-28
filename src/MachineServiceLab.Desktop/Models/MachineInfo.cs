namespace MachineServiceLab.Desktop.Models;

public sealed record MachineInfo(
    string Model,
    string SerialNumber,
    string FirmwareVersion);