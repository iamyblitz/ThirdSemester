namespace SimpleFTP;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a client that connects to an FTP-like server to list directories and download files over TCP.
/// </summary>
public class Client : IDisposable
{
    private readonly TcpClient tcpClient = new();
    private readonly string server;
    private readonly int port;

    public Client(string server, int port)
    {
        this.server = server;
        this.port = port;
    }

    /// <summary>
    /// Establishes a connection to the server asynchronously if not already connected.
    /// </summary>
    private async Task ConnectAsync()
    {
        if (!tcpClient.Connected)
        {
            try
            {
                await tcpClient.ConnectAsync(server, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Sends a request to list the contents of a directory on the server.
    /// Receives and returns the list of files and directories.
    /// </summary>
    /// <param name="directory">The path of the directory to list, relative to the server's base directory.</param>
    /// <returns>A task representing the asynchronous operation of listing the directory.</returns>
    public async Task<List<(string name, bool isDirectory)>> ListCommand(string directory)
    {
        await ConnectAsync();

        using var stream = tcpClient.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);

        directory = directory.Replace('\\', '/');
        await writer.WriteLineAsync($"1 {directory}");

        string response = await reader.ReadLineAsync();

        if (string.IsNullOrEmpty(response) || response == "-1")
        {
            Console.WriteLine("Directory doesn't exist.");
            return new List<(string name, bool isDirectory)>();
        }

        string[] parts = response.Split(' ');
        if (!int.TryParse(parts[0], out int count))
        {
            Console.WriteLine("Invalid response from server.");
            return new List<(string name, bool isDirectory)>();
        }

        var entries = new List<(string name, bool isDirectory)>();

        for (int i = 1; i < parts.Length; i += 2)
        {
            if (i + 1 >= parts.Length)
            {
                Console.WriteLine("Invalid response format.");
                return new List<(string name, bool isDirectory)>();
            }

            string name = parts[i];
            bool isDir = parts[i + 1] == "true";
            entries.Add((name, isDir));
        }

        return entries;
    }

    /// <summary>
    /// Sends a request to download a file from the server.
    /// Receives and saves the file to the local directory.
    /// </summary>
    /// <param name="filePath">The path of the file to download, relative to the server's base directory.</param>
    /// <returns>A task representing the asynchronous file download operation.</returns>
    public async Task GetCommand(string filePath)
    {
        await ConnectAsync();

        NetworkStream stream = tcpClient.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        
        filePath = filePath.Replace('\\', '/');
        await writer.WriteLineAsync($"2 {filePath}");
        
        StringBuilder sizeBuilder = new StringBuilder();
        byte[] buffer = new byte[1];
        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, 1);
            if (bytesRead == 0)
            {
                Console.WriteLine("Connection closed.");
                return;
            }
            char c = (char)buffer[0];
            if (c == ' ')
            {
                break;
            }
            sizeBuilder.Append(c);
        }

        if (!long.TryParse(sizeBuilder.ToString(), out long size))
        {
            Console.WriteLine("Invalid response from server.");
            return;
        }

        if (size == -1)
        {
            Console.WriteLine("File does not exist.");
            return;
        }
        
        byte[] fileBuffer = new byte[size];
        int totalBytesRead = 0;
        while (totalBytesRead < size)
        {
            int bytesRead = await stream.ReadAsync(fileBuffer, totalBytesRead, (int)(size - totalBytesRead));
            if (bytesRead == 0)
            {
                Console.WriteLine("Connection closed unexpectedly.");
                return;
            }
            totalBytesRead += bytesRead;
        }

        string fileName = Path.GetFileName(filePath);
        await File.WriteAllBytesAsync(fileName, fileBuffer);
        Console.WriteLine($"File '{fileName}' downloaded successfully.");
    }

    /// <summary>
    /// Releases all resources used by the <see cref="Client"/> class.
    /// Closes the TCP connection to the server.
    /// </summary>
    public void Dispose()
    {
        tcpClient?.Close();
    }
}

