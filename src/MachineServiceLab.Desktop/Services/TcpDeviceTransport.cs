using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MachineServiceLab.Desktop.Models;

namespace MachineServiceLab.Desktop.Services;

public sealed class TcpDeviceTransport : IDeviceTransport
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task<MachineInfo> ConnectAsync()
    {
        _client = new TcpClient();

        await _client.ConnectAsync("localhost", 7001);

        var stream = _client.GetStream();

        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream)
        {
            AutoFlush = true
        };

        var response = await SendAsync("INFO");
        var parts = response.Split('|');

        return new MachineInfo(
            Model: parts[1],
            SerialNumber: parts[2],
            FirmwareVersion: parts[3]);
    }

    public async Task DisconnectAsync()
    {
        if (_client is null)
        {
            return;
        }

        await SendAsync("DISCONNECT");

        _reader?.Dispose();
        _writer?.Dispose();
        _client.Dispose();

        _reader = null;
        _writer = null;
        _client = null;
    }

    public async Task<DiagnosticsSnapshot> ReadDiagnosticsAsync()
    {
        var response = await SendAsync("DIAGNOSTICS");
        var parts = response.Split('|');

        return new DiagnosticsSnapshot(
            BatteryPercent: int.Parse(parts[1]),
            BatteryVoltage: double.Parse(parts[2]),
            ControllerTemperatureC: double.Parse(parts[3]),
            MachineHours: double.Parse(parts[4]),
            FaultCodes: parts[5].Split(';'));
    }

    public async Task<MachineConfiguration> ReadConfigurationAsync()
    {
        var response = await SendAsync("GET_CONFIG");
        var parts = response.Split('|');

        return new MachineConfiguration(
            EcoMode: bool.Parse(parts[1]),
            BrushPressureLevel: int.Parse(parts[2]),
            MaxSpeedPercent: int.Parse(parts[3]));
    }

    public async Task UpdateConfigurationAsync(
        MachineConfiguration configuration)
    {
        await SendAsync(
            $"SET_CONFIG|{configuration.EcoMode}|{configuration.BrushPressureLevel}|{configuration.MaxSpeedPercent}");
    }

    public async Task<string> UpdateFirmwareAsync(
        IProgress<int> progress,
        CancellationToken cancellationToken)
    {
        EnsureConnected();

        await _writer!.WriteLineAsync("FIRMWARE");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response =
                await _reader!.ReadLineAsync(cancellationToken)
                ?? throw new IOException("Machine disconnected.");

            var parts = response.Split('|');

            if (parts[0] == "PROGRESS")
            {
                progress.Report(int.Parse(parts[1]));
                continue;
            }

            if (parts[0] == "FIRMWARE_COMPLETE")
            {
                return parts[1];
            }
        }
    }

    private async Task<string> SendAsync(string command)
    {
        EnsureConnected();

        await _writer!.WriteLineAsync(command);

        return await _reader!.ReadLineAsync()
            ?? throw new IOException("Machine disconnected.");
    }

    private void EnsureConnected()
    {
        if (_client is null || !_client.Connected)
        {
            throw new InvalidOperationException(
                "Machine is not connected.");
        }
    }
}