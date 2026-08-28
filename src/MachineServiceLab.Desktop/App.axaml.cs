using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MachineServiceLab.Desktop.Services;
using MachineServiceLab.Desktop.ViewModels;
using MachineServiceLab.Desktop.Views;

namespace MachineServiceLab.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new SimulatedDeviceTransport()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}