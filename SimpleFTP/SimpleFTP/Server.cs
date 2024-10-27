namespace SimpleFTP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// Represents an FTP-like server that handles client requests for file listings and file downloads over TCP.
/// </summary>
public class Server
{
    private readonly TcpListener listener;
    private readonly string baseDirectory;

    /// <summary>
    /// Constructor for initialization of listener and baseDirectory.
    /// </summary>
    /// <param name="port"></param>
    /// <param name="baseDirectory"></param>
    public Server(int port, string? baseDirectory = null)
    {
        listener = new TcpListener(IPAddress.Any, port);
        this.baseDirectory = baseDirectory ?? Directory.GetCurrentDirectory();
    }
    /// <summary>
    /// Starts the server, allowing it to accept and handle client connections asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of the server.</returns>
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
    
    
    /// <summary>
    /// Handles an individual client connection asynchronously. Processes commands for listing directory contents
    /// and retrieving file contents based on client requests.
    /// </summary>
    /// <param name="client">The client connection to be handled.</param>
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

    /// <summary>
    /// Sends a listing of files and directories within the specified path to the client.
    /// If the directory does not exist, sends a response indicating an error.
    /// </summary>
    /// <param name="writer">The writer used to send data to the client.</param>
    /// <param name="path">The path of the directory to list, relative to the server's base directory.</param>
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
    
    /// <summary>
    /// Sends the content of the specified file to the client. 
    /// If the file does not exist, sends a response indicating an error.
    /// </summary>
    /// <param name="writer">The writer used to send data to the client.</param>
    /// <param name="stream">The network stream for sending file data directly to the client.</param>
    /// <param name="path">The path of the file to retrieve, relative to the server's base directory.</param>
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