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
}