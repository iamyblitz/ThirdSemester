// <copyright file="Server.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
    private bool isRunning = true;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Constructor for initialization of listener and baseDirectory.
    /// </summary>
    public Server(int port, string? baseDirectory = null)
    {
        listener = new TcpListener(IPAddress.Any, port);
        Console.WriteLine("basedir:" + baseDirectory);
        this.baseDirectory = baseDirectory ?? Environment.CurrentDirectory;
    }

    /// <summary>
    /// Starts the server, allowing it to accept and handle client connections asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of the server.</returns>
    public async Task StartAsync()
    {
        listener.Start();
        try
        {
            while (isRunning)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
            }
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Server stopped due to listener death.");
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    /// <summary>
    /// Handles an individual client connection asynchronously. Processes commands for listing directory contents
    /// and retrieving file contents based on client requests.
    /// </summary>
    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            await using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream);
            await using var writer = new StreamWriter(stream) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(request))
                        return;

                    var parts = request.Split(' ');

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
            }
        }
    }

    /// <summary>
    /// Sends a listing of files and directories within the specified path to the client.
    /// If the directory does not exist, sends a response indicating an error.
    /// </summary>
    private async Task HandleListCommandAsync(StreamWriter writer, string path)
    {
        string fullPath = Path.Combine(baseDirectory, path);
        if (!Directory.Exists(fullPath))
        {
            await writer.WriteLineAsync("-1");
            return;
        }

        var entries = Directory.GetFileSystemEntries(fullPath);
        string response = $"{entries.Length}";
        await writer.WriteLineAsync(response);
        foreach (var entry in entries)
        {
            var name = Path.GetFileName(entry);
            var isDir = Directory.Exists(entry) ? "true" : "false";
            await writer.WriteLineAsync($"{name} {isDir}");
        }
    }

    /// <summary>
    /// Sends the content of the specified file to the client. 
    /// If the file does not exist, sends a response indicating an error.
    /// </summary>
    private async Task HandleGetCommandAsync(StreamWriter writer, NetworkStream stream, string path)
    {
        string fullPath = Path.Combine(baseDirectory, path);
        Console.WriteLine(path);
        if (!File.Exists(fullPath))
        {
            await writer.WriteLineAsync("-1\n");
            return;
        }

        var fileBytes = await File.ReadAllBytesAsync(fullPath);
        await writer.WriteLineAsync($"{fileBytes.Length}");
        await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void StopServer()
    {
        _cancellationTokenSource.Cancel();
        listener.Stop();
        isRunning = false;
    }
}