namespace SimpleFTP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Server(int port)
{
    private readonly TcpListener listener = new(IPAddress.Any, port);

    public async Task StartAsync()
    {
        listener.Start();
        Console.WriteLine("Server started.");
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        await using NetworkStream stream = client.GetStream();
        using var reader = new StreamReader(stream);
        await using var writer = new StreamWriter(stream);
        writer.AutoFlush = true;

        try
        {
            var request = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(request))
                return;
            
            var parts = request.Split(' ', 2);
            if (parts.Length < 2) return;
            
            string command = parts[0];
            string path = parts[1].Trim();

            switch (command)
            {
                case "1":
                    await HandleListCommandAsync(writer, path);
                    break;
                case "2":
                    await HandleGetCommandAsync(writer, stream, path);
                    break;
                default:
                    Console.WriteLine("Invalid command received.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Problem processing the client: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static async Task HandleListCommandAsync(StreamWriter writer, string path)
    {
        if (!Directory.Exists(path))
        {
            await writer.WriteLineAsync("size -1");
            return;
        }
       
        var entries = Directory.GetFileSystemEntries(path);
        string response = $"size {entries.Length}";

        foreach (var entry in entries)
        {
            var name = Path.GetFileName(entry);
            var isDir = Directory.Exists(entry) ? "true" : "false";
            response += $" {name} {isDir}";
        }
        await writer.WriteAsync(response);
        await writer.WriteLineAsync();
    }
    
    private static async Task HandleGetCommandAsync(StreamWriter writer, NetworkStream stream, string path)
    {
        if (!File.Exists(path))
        {
            await writer.WriteLineAsync("size -1");
            return;
        }
       
        var fileBytes = await File.ReadAllBytesAsync(path);
        await writer.WriteLineAsync($"size {fileBytes.Length}");
        await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
        await stream.FlushAsync();
    }
}