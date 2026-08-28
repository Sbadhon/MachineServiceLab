using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Services;

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

    public IAsyncRelayCommand ConnectCommand { get; }

    public MainViewModel(IDeviceTransport deviceTransport)
    {
        _deviceTransport = deviceTransport;

        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
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
}