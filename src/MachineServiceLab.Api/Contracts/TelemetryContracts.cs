using System.ComponentModel.DataAnnotations;

namespace MachineServiceLab.Api.Contracts;

public sealed record TelemetryRequest(
    [Required]
    [StringLength(50, MinimumLength = 3)]
    string SerialNumber,

    [Required]
    [StringLength(100)]
    string Metric,

    double Value,

    [Required]
    [StringLength(20)]
    string Unit);

public sealed record TelemetryResponse(
    int Id,
    string SerialNumber,
    string Metric,
    double Value,
    string Unit,
    DateTimeOffset CapturedAt);