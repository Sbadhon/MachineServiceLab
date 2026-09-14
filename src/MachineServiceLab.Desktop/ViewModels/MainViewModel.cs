using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MachineServiceLab.Desktop.Models;
using MachineServiceLab.Desktop.Services;

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

    public IAsyncRelayCommand ConnectCommand { get; }
    public IAsyncRelayCommand DisconnectCommand { get; }
    public IAsyncRelayCommand ReadDiagnosticsCommand { get; }
    public IAsyncRelayCommand LoadConfigurationCommand { get; }
    public IAsyncRelayCommand SaveConfigurationCommand { get; }
    public IAsyncRelayCommand UpdateFirmwareCommand { get; }
    public IRelayCommand CancelFirmwareCommand { get; }

    public MainViewModel(
        IDeviceTransport deviceTransport,
        CloudApiClient cloudApiClient)
    {
        _deviceTransport = deviceTransport;
        _cloudApiClient = cloudApiClient;

        ConnectCommand =
            new AsyncRelayCommand(ConnectAsync);

        DisconnectCommand =
            new AsyncRelayCommand(DisconnectAsync);

        ReadDiagnosticsCommand =
            new AsyncRelayCommand(ReadDiagnosticsAsync);

        LoadConfigurationCommand =
            new AsyncRelayCommand(LoadConfigurationAsync);

        SaveConfigurationCommand =
            new AsyncRelayCommand(SaveConfigurationAsync);

        UpdateFirmwareCommand =
            new AsyncRelayCommand(UpdateFirmwareAsync);

        CancelFirmwareCommand =
            new RelayCommand(
                () => UpdateFirmwareCommand.Cancel());
    }

    private async Task ConnectAsync()
    {
        ErrorMessage = "";
        ConnectionStatus = "Connecting...";

        MachineInfo machine;

        try
        {
            machine = await _deviceTransport.ConnectAsync();
        }
        catch (Exception ex)
        {
            ResetConnectionState();

            ConnectionStatus = "Connection failed";
            ErrorMessage = ex.Message;

            return;
        }

        Model = machine.Model;
        SerialNumber = machine.SerialNumber;
        FirmwareVersion = machine.FirmwareVersion;

        IsConnected = true;
        ConnectionStatus = "Connected";

        try
        {
            await _cloudApiClient.RegisterMachineAsync(machine);
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Machine connected. Cloud registration failed: {ex.Message}";
        }
    }

    private async Task DisconnectAsync()
    {
        ErrorMessage = "";
        ConnectionStatus = "Disconnecting...";

        try
        {
            await _deviceTransport.DisconnectAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Disconnect warning: {ex.Message}";
        }
        finally
        {
            ResetConnectionState();

            ConnectionStatus = "Disconnected";
            FirmwareUpdateStatus = "Ready";
        }
    }

    private async Task ReadDiagnosticsAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        ErrorMessage = "";

        DiagnosticsSnapshot diagnostics;

        try
        {
            diagnostics =
                await _deviceTransport.ReadDiagnosticsAsync();
        }
        catch (Exception ex)
        {
            HandleDeviceFailure(ex);
            return;
        }

        Battery =
            $"{diagnostics.BatteryPercent}% / " +
            $"{diagnostics.BatteryVoltage:F1} V";

        ControllerTemperature =
            $"{diagnostics.ControllerTemperatureC:F1} °C";

        MachineHours =
            $"{diagnostics.MachineHours:F1}";

        Faults =
            string.Join(
                Environment.NewLine,
                diagnostics.FaultCodes);

        try
        {
            await Task.WhenAll(
                _cloudApiClient.UploadDiagnosticsAsync(
                    SerialNumber,
                    diagnostics),

                _cloudApiClient.UploadTelemetryAsync(
                    SerialNumber,
                    "BatteryVoltage",
                    diagnostics.BatteryVoltage,
                    "V"),

                _cloudApiClient.UploadTelemetryAsync(
                    SerialNumber,
                    "ControllerTemperature",
                    diagnostics.ControllerTemperatureC,
                    "C"),

                _cloudApiClient.UploadTelemetryAsync(
                    SerialNumber,
                    "MachineHours",
                    diagnostics.MachineHours,
                    "hours"));
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Diagnostics completed. Cloud sync failed: {ex.Message}";
        }
    }

    private async Task LoadConfigurationAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        ErrorMessage = "";

        try
        {
            var configuration =
                await _deviceTransport.ReadConfigurationAsync();

            EcoMode = configuration.EcoMode;
            BrushPressureLevel =
                configuration.BrushPressureLevel;
            MaxSpeedPercent =
                configuration.MaxSpeedPercent;

            ConfigurationStatus =
                "Configuration loaded";
        }
        catch (Exception ex)
        {
            HandleDeviceFailure(ex);
        }
    }

    private async Task SaveConfigurationAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        ErrorMessage = "";

        var configuration = new MachineConfiguration(
            EcoMode,
            BrushPressureLevel,
            MaxSpeedPercent);

        try
        {
            await _deviceTransport
                .UpdateConfigurationAsync(configuration);

            ConfigurationStatus =
                "Configuration saved";
        }
        catch (Exception ex)
        {
            HandleDeviceFailure(ex);
        }
    }

    private async Task UpdateFirmwareAsync(
        CancellationToken cancellationToken)
    {
        if (!IsConnected || IsFirmwareUpdating)
        {
            return;
        }

        ErrorMessage = "";
        IsFirmwareUpdating = true;
        FirmwareProgress = 0;
        FirmwareUpdateStatus =
            "Programming firmware...";

        try
        {
            var progress = new Progress<int>(
                value => FirmwareProgress = value);

            var newVersion =
                await _deviceTransport.UpdateFirmwareAsync(
                    progress,
                    cancellationToken);

            FirmwareVersion = newVersion;
            FirmwareUpdateStatus =
                "Firmware update completed";
        }
        catch (OperationCanceledException)
        {
            FirmwareUpdateStatus =
                "Firmware update cancelled";

            ResetConnectionState();

            ConnectionStatus =
                "Disconnected - reconnect required";
        }
        catch (Exception ex)
        {
            FirmwareUpdateStatus =
                $"Firmware update failed: {ex.Message}";

            HandleDeviceFailure(ex);
        }
        finally
        {
            IsFirmwareUpdating = false;
        }
    }

    private void HandleDeviceFailure(Exception exception)
    {
        ResetConnectionState();

        ConnectionStatus = "Connection lost";
        ErrorMessage = exception.Message;
    }

    private void ResetConnectionState()
    {
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