using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public sealed class CloudApiClient
{
    private readonly HttpClient _httpClient;

    public CloudApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task RegisterMachineAsync(
        MachineInfo machine,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/machines",
            machine,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task UploadDiagnosticsAsync(
        string serialNumber,
        DiagnosticsSnapshot diagnostics,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            SerialNumber = serialNumber,
            diagnostics.BatteryPercent,
            diagnostics.BatteryVoltage,
            diagnostics.ControllerTemperatureC,
            diagnostics.MachineHours,
            diagnostics.FaultCodes
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/diagnostics",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task UploadTelemetryAsync(
        string serialNumber,
        string metric,
        double value,
        string unit,
        CancellationToken cancellationToken = default)
        {
            var request = new
            {
                SerialNumber = serialNumber,
                Metric = metric,
                Value = value,
                Unit = unit
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/telemetry",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();
    }
}