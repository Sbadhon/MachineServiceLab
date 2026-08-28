using System;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public sealed class SimulatedDeviceTransport : IDeviceTransport
{
    private bool _isConnected;

    private MachineConfiguration _configuration =
    new(
        EcoMode: true,
        BrushPressureLevel: 2,
        MaxSpeedPercent: 80);


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
        EnsureConnected();
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

    public async Task<MachineConfiguration> ReadConfigurationAsync()
    {
        EnsureConnected();

        await Task.Delay(500);

        return _configuration;
    }

    public async Task UpdateConfigurationAsync(
        MachineConfiguration configuration)
    {
        EnsureConnected();

        await Task.Delay(750);

        _configuration = configuration;
    }

    private void EnsureConnected()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Machine is not connected.");
        }
    }

    public async Task<string> UpdateFirmwareAsync(IProgress<int> progress)
    {
        EnsureConnected();

        for (var percent = 10; percent <= 100; percent += 10)
        {
            await Task.Delay(300);
            progress.Report(percent);
        }

        return "1.1.0";
    }
}