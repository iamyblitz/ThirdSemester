namespace SimpleFTP;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents an FTP-like server that handles client requests for file listings and file downloads over TCP.
/// </summary>
public class Server
{
    private readonly int port;
    private readonly string baseDirectory;
    private readonly TcpListener listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="port">The port on which the server listens for incoming connections.</param>
    /// <param name="baseDirectory">The base directory for file operations.</param>
    public Server(int port, string baseDirectory)
    {
        this.port = port;
        this.baseDirectory = baseDirectory;
        listener = new TcpListener(IPAddress.Any, port);
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
    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var request = await reader.ReadLineAsync();
            
            if (string.IsNullOrWhiteSpace(request))
                throw new InvalidOperationException("Received an empty request from client.");

            var parts = request.Split(' ', 2);
            if (parts.Length < 2)
                throw new InvalidOperationException("Request does not contain a valid command and path.");

            string command = parts[0];
            string path = parts[1].Trim();

            switch (command)
            {
                case "1":
                    await HandleListCommandAsync(stream, path);
                    break;
                case "2":
                    await HandleGetCommandAsync(stream, path);
                    break;
                default:
                    Console.WriteLine("Invalid command received.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Problem processing the request: {ex.Message}");
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
    /// <param name="stream">The network stream to send data to the client.</param>
    /// <param name="path">The path of the directory to list, relative to the server's base directory.</param>
    private async Task HandleListCommandAsync(NetworkStream stream, string path)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        
        var fullPath = path.Replace('/', Path.DirectorySeparatorChar);

        
        if (!Directory.Exists(fullPath))
        {
            await writer.WriteLineAsync("-1\n");
            await writer.FlushAsync();
            return;
        }
        
        var entries = Directory.GetFileSystemEntries(fullPath);
        StringBuilder response = new StringBuilder();
        response.Append(entries.Length);

        foreach (var entry in entries)
        {
            var name = Path.GetFileName(entry);
            var isDir = Directory.Exists(entry) ? "true" : "false";
            response.Append($" {name} {isDir}");
        }
        response.Append("\n");
        await writer.WriteLineAsync(response.ToString());
        await writer.FlushAsync();
    }

    /// <summary>
    /// Sends the content of the specified file to the client. 
    /// If the file does not exist, sends a response indicating an error.
    /// </summary>
    /// <param name="stream">The network stream for sending data directly to the client.</param>
    /// <param name="path">The path of the file to retrieve, relative to the server's base directory.</param>
    private async Task HandleGetCommandAsync(NetworkStream stream, string path)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        
        var fullPath = path.Replace('/', Path.DirectorySeparatorChar);
        if (!File.Exists(fullPath))
        {
            await writer.WriteLineAsync("-1\n");
            await writer.FlushAsync();
            return;
        }
        
        var fileBytes = await File.ReadAllBytesAsync(fullPath);
        byte[] sizeBytes = Encoding.ASCII.GetBytes($"{fileBytes.Length} ");
        await stream.WriteAsync(sizeBytes, 0, sizeBytes.Length);
        await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
        await stream.FlushAsync();
    }
}

