using System.ComponentModel.DataAnnotations;

namespace MachineServiceLab.Api.Contracts;

public sealed record DiagnosticsRequest(
    [Required]
    [StringLength(50, MinimumLength = 3)]
    string SerialNumber,

    [Range(0, 100)]
    int BatteryPercent,

    [Range(0, 100)]
    double BatteryVoltage,

    [Range(-50, 150)]
    double ControllerTemperatureC,

    [Range(0, 1_000_000)]
    double MachineHours,

    string[] FaultCodes);

public sealed record DiagnosticsResponse(
    int Id,
    string SerialNumber,
    int BatteryPercent,
    double BatteryVoltage,
    double ControllerTemperatureC,
    double MachineHours,
    string[] FaultCodes,
    DateTimeOffset CapturedAt);