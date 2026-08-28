using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public sealed class SimulatedDeviceTransport : IDeviceTransport
{
    public async Task<MachineInfo> ConnectAsync()
    {
        await Task.Delay(1000);

        return new MachineInfo(
            Model: "Scrubber-X1",
            SerialNumber: "MSL-100001",
            FirmwareVersion: "1.0.0");
    }
}