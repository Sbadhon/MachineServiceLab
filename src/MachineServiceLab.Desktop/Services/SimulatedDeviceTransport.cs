using System;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public sealed class SimulatedDeviceTransport : IDeviceTransport
{
    private bool _isConnected;

    public async Task<MachineInfo> ConnectAsync()
    {
        await Task.Delay(1000);

        _isConnected = true;

        return new MachineInfo(
            Model: "Scrubber-X1",
            SerialNumber: "MSL-100001",
            FirmwareVersion: "1.0.0");
    }

    public async Task<DiagnosticsSnapshot> ReadDiagnosticsAsync()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Machine is not connected.");
        }
        await Task.Delay(750);

        return new DiagnosticsSnapshot(
            BatteryPercent: 81,
            BatteryVoltage: 37.8,
            ControllerTemperatureC: 42.5,
            MachineHours: 1432.7,
            FaultCodes:
            [
                "F102 - Brush Motor Overcurrent",
                "F208 - Battery Voltage Low"
            ]);
    }

    public async Task DisconnectAsync()
    {
        await Task.Delay(300);

        _isConnected = false;
    }
}