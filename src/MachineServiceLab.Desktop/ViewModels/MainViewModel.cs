using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Services;
using System;

namespace MachineServiceLab.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDeviceTransport _deviceTransport;

    [ObservableProperty]
    public partial string ConnectionStatus { get; set; } = "Disconnected";

    [ObservableProperty]
    public partial string Model { get; set; } = "-";

    [ObservableProperty]
    public partial string SerialNumber { get; set; } = "-";

    [ObservableProperty]
    public partial string FirmwareVersion { get; set; } = "-";

    [ObservableProperty]
    public partial string Battery { get; set; } = "-";

    [ObservableProperty]
    public partial string ControllerTemperature { get; set; } = "-";

    [ObservableProperty]
    public partial string MachineHours { get; set; } = "-";

    [ObservableProperty]
    public partial string Faults { get; set; } = "-";

    public IAsyncRelayCommand ReadDiagnosticsCommand { get; }

    public IAsyncRelayCommand ConnectCommand { get; }

    public MainViewModel(IDeviceTransport deviceTransport)
    {
        _deviceTransport = deviceTransport;

        ConnectCommand = new AsyncRelayCommand(ConnectAsync);

        ReadDiagnosticsCommand = new AsyncRelayCommand(ReadDiagnosticsAsync);
    }

    private async Task ConnectAsync()
    {
        ConnectionStatus = "Connecting...";

        var machine = await _deviceTransport.ConnectAsync();

        Model = machine.Model;
        SerialNumber = machine.SerialNumber;
        FirmwareVersion = machine.FirmwareVersion;

        ConnectionStatus = "Connected";
    }

    private async Task ReadDiagnosticsAsync()
    {
        var diagnostics = await _deviceTransport.ReadDiagnosticsAsync();

        Battery = $"{diagnostics.BatteryPercent}% / {diagnostics.BatteryVoltage:F1} V";
        ControllerTemperature = $"{diagnostics.ControllerTemperatureC:F1} °C";
        MachineHours = $"{diagnostics.MachineHours:F1}";
        Faults = string.Join(Environment.NewLine, diagnostics.FaultCodes);
    }
}