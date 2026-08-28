using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public interface IDeviceTransport
{
    Task<MachineInfo> ConnectAsync();
    Task<DiagnosticsSnapshot> ReadDiagnosticsAsync();
}