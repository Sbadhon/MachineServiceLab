using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public sealed class TcpDeviceTransport : IDeviceTransport
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task<MachineInfo> ConnectAsync()
    {
        await CleanupAsync();

        _client = new TcpClient();

        using var timeout = new CancellationTokenSource(Timeout);

        try
        {
            await _client.ConnectAsync(
                "localhost",
                7001,
                timeout.Token);

        var stream = _client.GetStream();

        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream)
        {
            AutoFlush = true
        };

            var response = await SendAsync("INFO");
            var parts = response.Split('|');

            if (parts.Length != 4 || parts[0] != "INFO")
            {
                throw new InvalidDataException(
                    "Invalid machine information response.");
            }

            return new MachineInfo(
                parts[1],
                parts[2],
                parts[3]);
        }
        catch
        {
            await CleanupAsync();
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_client is null)
        {
            return;
        }

        try
        {
            await SendAsync("DISCONNECT");
        }
        catch
        {
            // Connection may already be gone.
        }
        finally
        {
            await CleanupAsync();
        }
    }

    public async Task<DiagnosticsSnapshot> ReadDiagnosticsAsync()
    {
        var response = await SendAsync("DIAGNOSTICS");
        var parts = response.Split('|');

        if (parts.Length != 6 || parts[0] != "DIAGNOSTICS")
        {
            throw new InvalidDataException(
                "Invalid diagnostics response.");
        }

        return new DiagnosticsSnapshot(
            int.Parse(parts[1], CultureInfo.InvariantCulture),
            double.Parse(parts[2], CultureInfo.InvariantCulture),
            double.Parse(parts[3], CultureInfo.InvariantCulture),
            double.Parse(parts[4], CultureInfo.InvariantCulture),
            parts[5].Split(';'));
    }

    public async Task<MachineConfiguration> ReadConfigurationAsync()
    {
        var response = await SendAsync("GET_CONFIG");
        var parts = response.Split('|');

        if (parts.Length != 4 || parts[0] != "CONFIG")
        {
            throw new InvalidDataException(
                "Invalid configuration response.");
        }

        return new MachineConfiguration(
            bool.Parse(parts[1]),
            int.Parse(parts[2], CultureInfo.InvariantCulture),
            int.Parse(parts[3], CultureInfo.InvariantCulture));
    }

    public async Task UpdateConfigurationAsync(
        MachineConfiguration configuration)
    {
        var response = await SendAsync(
            $"SET_CONFIG|{configuration.EcoMode}|{configuration.BrushPressureLevel}|{configuration.MaxSpeedPercent}");

        if (response != "OK")
        {
            throw new InvalidDataException(
                "Machine rejected configuration update.");
        }
    }

    public async Task<string> UpdateFirmwareAsync(
        IProgress<int> progress,
        CancellationToken cancellationToken)
    {
        EnsureConnected();

        await _writer!.WriteLineAsync("FIRMWARE");

        while (true)
        {
            var response = await ReadLineAsync(cancellationToken);
            var parts = response.Split('|');

            if (parts.Length == 2 &&
                parts[0] == "PROGRESS" &&
                int.TryParse(parts[1], out var percent))
            {
                progress.Report(percent);
                continue;
            }

            if (parts.Length == 2 &&
                parts[0] == "FIRMWARE_COMPLETE")
            {
                return parts[1];
            }

            throw new InvalidDataException(
                "Invalid firmware response.");
        }
    }

    private async Task<string> SendAsync(string command)
    {
        EnsureConnected();

        try
        {
            await _writer!.WriteLineAsync(command);

            using var timeout =
                new CancellationTokenSource(Timeout);

            return await ReadLineAsync(timeout.Token);
        }
        catch
        {
            await CleanupAsync();
            throw;
        }
    }

    private async Task<string> ReadLineAsync(
        CancellationToken cancellationToken)
    {
        EnsureConnected();

        var response =
            await _reader!.ReadLineAsync(cancellationToken);

        return response ??
            throw new IOException(
                "Machine connection was lost.");
    }

    private void EnsureConnected()
    {
        if (_client is null ||
            _reader is null ||
            _writer is null ||
            !_client.Connected)
        {
            throw new InvalidOperationException(
                "Machine is not connected.");
        }
    }

    private Task CleanupAsync()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Dispose();

        _reader = null;
        _writer = null;
        _client = null;

        return Task.CompletedTask;
    }
}