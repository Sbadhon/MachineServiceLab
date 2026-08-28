using System;
using System.Threading;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public interface IDeviceTransport
{
    Task<MachineInfo> ConnectAsync();
    Task<DiagnosticsSnapshot> ReadDiagnosticsAsync();
    Task DisconnectAsync();
    Task<MachineConfiguration> ReadConfigurationAsync();
    Task UpdateConfigurationAsync(MachineConfiguration configuration);
    Task<string> UpdateFirmwareAsync(
        IProgress<int> progress,
        CancellationToken cancellationToken);
}