using System.Globalization;
using System.Net;
using System.Net.Sockets;

var listener =
    new TcpListener(IPAddress.Loopback, 7001);

var ecoMode = true;
var brushPressure = 2;
var maxSpeed = 80;
var firmwareVersion = "1.0.0";

listener.Start();

Console.WriteLine(
    "Machine Simulator listening on localhost:7001");

while (true)
{
    using var client =
        await listener.AcceptTcpClientAsync();

    Console.WriteLine("Desktop connected");

    using var stream = client.GetStream();
    using var reader = new StreamReader(stream);
    using var writer = new StreamWriter(stream)
    {
        AutoFlush = true
    };

    while (true)
    {
        var command = await reader.ReadLineAsync();

        if (command is null)
        {
            break;
        }

        Console.WriteLine($"Received: {command}");

        if (command == "INFO")
        {
            await writer.WriteLineAsync(
                $"INFO|Scrubber-X1|MSL-100001|{firmwareVersion}");

            continue;
        }

        if (command == "DIAGNOSTICS")
        {
            await writer.WriteLineAsync(
                "DIAGNOSTICS|81|37.8|42.5|1432.7|" +
                "F102 - Brush Motor Overcurrent;" +
                "F208 - Battery Voltage Low");

            continue;
        }

        if (command == "GET_CONFIG")
        {
            await writer.WriteLineAsync(
                $"CONFIG|{ecoMode}|{brushPressure}|{maxSpeed}");

            continue;
        }

        if (command.StartsWith(
            "SET_CONFIG|",
            StringComparison.Ordinal))
        {
            var parts = command.Split('|');

            if (parts.Length != 4 ||
                !bool.TryParse(parts[1], out var requestedEcoMode) ||
                !int.TryParse(
                    parts[2],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var requestedBrushPressure) ||
                !int.TryParse(
                    parts[3],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var requestedMaxSpeed))
            {
                await writer.WriteLineAsync("ERROR|INVALID_CONFIG");
                continue;
            }

            ecoMode = requestedEcoMode;
            brushPressure = requestedBrushPressure;
            maxSpeed = requestedMaxSpeed;

            await writer.WriteLineAsync("OK");
            continue;
        }

        if (command == "FIRMWARE")
        {
            for (var progress = 10;
                 progress <= 100;
                 progress += 10)
            {
                await Task.Delay(300);

                await writer.WriteLineAsync(
                    $"PROGRESS|{progress}");
            }

            firmwareVersion = "1.1.0";

            await writer.WriteLineAsync(
                $"FIRMWARE_COMPLETE|{firmwareVersion}");

            continue;
        }

        if (command == "DISCONNECT")
        {
            await writer.WriteLineAsync("BYE");
            break;
        }

        await writer.WriteLineAsync("ERROR|UNKNOWN_COMMAND");
    }

    Console.WriteLine("Desktop disconnected");
}