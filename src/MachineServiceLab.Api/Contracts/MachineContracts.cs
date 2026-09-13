using System.ComponentModel.DataAnnotations;

namespace MachineServiceLab.Api.Contracts;

public sealed record RegisterMachineRequest(
    [Required]
    [StringLength(50, MinimumLength = 3)]
    string SerialNumber,

    [Required]
    [StringLength(100)]
    string Model,

    [Required]
    [StringLength(50)]
    string FirmwareVersion);

public sealed record MachineResponse(
    string SerialNumber,
    string Model,
    string FirmwareVersion,
    DateTimeOffset RegisteredAt);