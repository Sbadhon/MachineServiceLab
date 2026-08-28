using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace MachineServiceLab.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ConnectionStatus { get; set; } = "Disconnected";

    public IAsyncRelayCommand ConnectCommand { get; }

    public MainViewModel()
    {
        ConnectCommand = new AsyncRelayCommand(async () =>
        {
            ConnectionStatus = "Connecting...";

            await Task.Delay(1500);

            ConnectionStatus = "Connected";
        });
    }
}