using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Services;
using System;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDeviceTransport _deviceTransport;

    [ObservableProperty]
    public partial string ConnectionStatus { get; set; } = "Disconnected";

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

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

    [ObservableProperty]
    public partial bool EcoMode { get; set; }

    [ObservableProperty]
    public partial int BrushPressureLevel { get; set; }

    [ObservableProperty]
    public partial int MaxSpeedPercent { get; set; }

    [ObservableProperty]
    public partial string ConfigurationStatus { get; set; } = "-";

    public IAsyncRelayCommand LoadConfigurationCommand { get; }
    public IAsyncRelayCommand SaveConfigurationCommand { get; }
    public IAsyncRelayCommand ConnectCommand { get; }
    public IAsyncRelayCommand DisconnectCommand { get; }
    public IAsyncRelayCommand ReadDiagnosticsCommand { get; }

    public MainViewModel(IDeviceTransport deviceTransport)
    {
        _deviceTransport = deviceTransport;

        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync);
        ReadDiagnosticsCommand = new AsyncRelayCommand(ReadDiagnosticsAsync);
        LoadConfigurationCommand = new AsyncRelayCommand(LoadConfigurationAsync);
        SaveConfigurationCommand = new AsyncRelayCommand(SaveConfigurationAsync);
    }

    private async Task ConnectAsync()
    {
        ConnectionStatus = "Connecting...";

        var machine = await _deviceTransport.ConnectAsync();

        Model = machine.Model;
        SerialNumber = machine.SerialNumber;
        FirmwareVersion = machine.FirmwareVersion;

        IsConnected = true;
        ConnectionStatus = "Connected";
    }

    private async Task DisconnectAsync()
    {
        ConnectionStatus = "Disconnecting...";

        await _deviceTransport.DisconnectAsync();

        IsConnected = false;
        ConnectionStatus = "Disconnected";

        Model = "-";
        SerialNumber = "-";
        FirmwareVersion = "-";
        Battery = "-";
        ControllerTemperature = "-";
        MachineHours = "-";
        Faults = "-";
        EcoMode = false;
        BrushPressureLevel = 0;
        MaxSpeedPercent = 0;
        ConfigurationStatus = "-";
    }

    private async Task ReadDiagnosticsAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        var diagnostics = await _deviceTransport.ReadDiagnosticsAsync();

        Battery = $"{diagnostics.BatteryPercent}% / {diagnostics.BatteryVoltage:F1} V";
        ControllerTemperature = $"{diagnostics.ControllerTemperatureC:F1} °C";
        MachineHours = $"{diagnostics.MachineHours:F1}";
        Faults = string.Join(Environment.NewLine, diagnostics.FaultCodes);
    }

    private async Task LoadConfigurationAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        var configuration =
            await _deviceTransport.ReadConfigurationAsync();

        EcoMode = configuration.EcoMode;
        BrushPressureLevel = configuration.BrushPressureLevel;
        MaxSpeedPercent = configuration.MaxSpeedPercent;

        ConfigurationStatus = "Configuration loaded";
    }

    private async Task SaveConfigurationAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        var configuration = new MachineConfiguration(
            EcoMode,
            BrushPressureLevel,
            MaxSpeedPercent);

        await _deviceTransport.UpdateConfigurationAsync(configuration);

        ConfigurationStatus = "Configuration saved";
    }
}