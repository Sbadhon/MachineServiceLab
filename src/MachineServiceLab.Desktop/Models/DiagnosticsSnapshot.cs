namespace MachineServiceLab.Desktop.Models;

public sealed record DiagnosticsSnapshot(
    int BatteryPercent,
    double BatteryVoltage,
    double ControllerTemperatureC,
    double MachineHours,
    string[] FaultCodes);