using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Services;
using System;
using MachineServiceLab.Desktop.Models;
using System.Threading;

namespace MachineServiceLab.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDeviceTransport _deviceTransport;
    private readonly CloudApiClient _cloudApiClient;

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
    [ObservableProperty]
    public partial int FirmwareProgress { get; set; }

    [ObservableProperty]
    public partial string FirmwareUpdateStatus { get; set; } = "Ready";

    [ObservableProperty]
    public partial bool IsFirmwareUpdating { get; set; }
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = "";
    public IAsyncRelayCommand UpdateFirmwareCommand { get; }
    public IRelayCommand CancelFirmwareCommand { get; }
    public IAsyncRelayCommand LoadConfigurationCommand { get; }
    public IAsyncRelayCommand SaveConfigurationCommand { get; }
    public IAsyncRelayCommand ConnectCommand { get; }
    public IAsyncRelayCommand DisconnectCommand { get; }
    public IAsyncRelayCommand ReadDiagnosticsCommand { get; }

    public MainViewModel(IDeviceTransport deviceTransport, CloudApiClient cloudApiClient)
    {
        _deviceTransport = deviceTransport;
        _cloudApiClient = cloudApiClient;

        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync);
        ReadDiagnosticsCommand = new AsyncRelayCommand(ReadDiagnosticsAsync);
        LoadConfigurationCommand = new AsyncRelayCommand(LoadConfigurationAsync);
        SaveConfigurationCommand = new AsyncRelayCommand(SaveConfigurationAsync);
        UpdateFirmwareCommand = new AsyncRelayCommand(UpdateFirmwareAsync);
        CancelFirmwareCommand = new RelayCommand(() => UpdateFirmwareCommand.Cancel());
    }

    private async Task ConnectAsync()
    {
        ErrorMessage = "";
        ConnectionStatus = "Connecting...";

        try
        {
            var machine = await _deviceTransport.ConnectAsync();

            await _cloudApiClient.RegisterMachineAsync(machine);

            Model = machine.Model;
            SerialNumber = machine.SerialNumber;
            FirmwareVersion = machine.FirmwareVersion;

            IsConnected = true;
            ConnectionStatus = "Connected";
        }
        catch (Exception ex)
        {
            ResetConnectionState();

            ConnectionStatus = "Connection failed";
            ErrorMessage = ex.Message;
        }
    }

    private async Task DisconnectAsync()
    {
        ConnectionStatus = "Disconnecting...";

        await _deviceTransport.DisconnectAsync();

        IsConnected = false;
        ResetConnectionState();
        ConnectionStatus = "Disconnected";
        ErrorMessage = "";
        FirmwareUpdateStatus = "Ready";

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
        FirmwareProgress = 0;
        FirmwareUpdateStatus = "Ready";
        IsFirmwareUpdating = false;
    }

    private async Task ReadDiagnosticsAsync() {
        if (!IsConnected)
        {
            return;
        }

        ErrorMessage = "";

        try
        {
            var diagnostics =
                await _deviceTransport.ReadDiagnosticsAsync();

            await _cloudApiClient.UploadDiagnosticsAsync(
                SerialNumber,
                diagnostics);

            await _cloudApiClient.UploadTelemetryAsync(
                SerialNumber,
                "BatteryVoltage",
                diagnostics.BatteryVoltage,
                "V");

            await _cloudApiClient.UploadTelemetryAsync(
                SerialNumber,
                "ControllerTemperature",
                diagnostics.ControllerTemperatureC,
                "C");

            await _cloudApiClient.UploadTelemetryAsync(
                SerialNumber,
                "MachineHours",
                diagnostics.MachineHours,
                "hours");

            Battery =
                $"{diagnostics.BatteryPercent}% / {diagnostics.BatteryVoltage:F1} V";

            ControllerTemperature =
                $"{diagnostics.ControllerTemperatureC:F1} °C";

            MachineHours =
                $"{diagnostics.MachineHours:F1}";

            Faults =
                string.Join(
                    Environment.NewLine,
                    diagnostics.FaultCodes);
        }
        catch (Exception ex)
        {
            ResetConnectionState();

            ConnectionStatus = "Connection lost";
            ErrorMessage = ex.Message;
        }
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

    private async Task UpdateFirmwareAsync(CancellationToken cancellationToken)
    {
        if (!IsConnected || IsFirmwareUpdating)
        {
            return;
        }

        IsFirmwareUpdating = true;
        FirmwareProgress = 0;
        FirmwareUpdateStatus = "Programming firmware...";

        try
        {
            var progress = new Progress<int>(
                value => FirmwareProgress = value);

            var newVersion =
                await _deviceTransport.UpdateFirmwareAsync(
                    progress,
                    cancellationToken);

            FirmwareVersion = newVersion;
            FirmwareUpdateStatus = "Firmware update completed";
        }
        catch (OperationCanceledException)
        {
            FirmwareUpdateStatus = "Firmware update cancelled";
        }
        catch (Exception ex)
        {
            FirmwareUpdateStatus =
                $"Firmware update failed: {ex.Message}";
        }
        finally
        {
            IsFirmwareUpdating = false;
        }
    }

    private void ResetConnectionState() {
        IsConnected = false;

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

        FirmwareProgress = 0;
        IsFirmwareUpdating = false;
    }
}