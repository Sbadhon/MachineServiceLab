using System.Net;
using System.Net.Sockets;

var listener = new TcpListener(IPAddress.Loopback, 7001);

listener.Start();

Console.WriteLine("Machine Simulator listening on localhost:7001");

while (true)
{
    using var client = await listener.AcceptTcpClientAsync();

    Console.WriteLine("Desktop connected");

    using var stream = client.GetStream();
    using var reader = new StreamReader(stream);
    using var writer = new StreamWriter(stream)
    {
        AutoFlush = true
    };

    var ecoMode = true;
    var brushPressure = 2;
    var maxSpeed = 80;
    var firmwareVersion = "1.0.0";

    while (client.Connected)
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
        }
        else if (command == "DIAGNOSTICS")
        {
            await writer.WriteLineAsync(
                "DIAGNOSTICS|81|37.8|42.5|1432.7|F102 - Brush Motor Overcurrent;F208 - Battery Voltage Low");
        }
        else if (command == "GET_CONFIG")
        {
            await writer.WriteLineAsync(
                $"CONFIG|{ecoMode}|{brushPressure}|{maxSpeed}");
        }
        else if (command.StartsWith("SET_CONFIG|"))
        {
            var parts = command.Split('|');

            ecoMode = bool.Parse(parts[1]);
            brushPressure = int.Parse(parts[2]);
            maxSpeed = int.Parse(parts[3]);

            await writer.WriteLineAsync("OK");
        }
        else if (command == "FIRMWARE")
        {
            for (var progress = 10; progress <= 100; progress += 10)
            {
                await Task.Delay(300);
                await writer.WriteLineAsync($"PROGRESS|{progress}");
            }

            firmwareVersion = "1.1.0";

            await writer.WriteLineAsync(
                $"FIRMWARE_COMPLETE|{firmwareVersion}");
        }
        else if (command == "DISCONNECT")
        {
            await writer.WriteLineAsync("BYE");
            break;
        }
    }

    Console.WriteLine("Desktop disconnected");
}