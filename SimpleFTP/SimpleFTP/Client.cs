namespace SimpleFTP;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

/// <summary>
/// Represents a client that connects to an FTP-like server to list directories and download files over TCP.
/// </summary>
public class Client(string server, int port)
{

    /// <summary>
    /// Establishes a connection to the server asynchronously if not already connected.
    /// </summary>
    private async Task ConnectAsync(TcpClient tcpClient)
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
    /// Receives and displays the list of files and directories.
    /// </summary>
    /// <param name="directory">The path of the directory to list, relative to the server's base directory.</param>
    /// <returns>A task representing the asynchronous operation of listing the directory.</returns>
    public async Task<String?> ListCommandAsync(string directory)
    {
        using TcpClient tcpClient = new TcpClient();
        await ConnectAsync(tcpClient);

        await using var stream = tcpClient.GetStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8)  { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        string request = $"1 {directory}";
        await writer.WriteLineAsync(request);

        string? response = await reader.ReadLineAsync();
        
        if (response == "-1")
        {
            return "Directory doesn't exist.";
        }
        StringBuilder resultBuilder = new StringBuilder();
        int.TryParse(response, out int entriesCount);
        for (int i = 0; i < entriesCount; i++)
        {
            string? entryLine = await reader.ReadLineAsync();
            resultBuilder.AppendLine(entryLine);
        }
        return resultBuilder.ToString();
    }
    
    /// <summary>
    /// Sends a request to download a file from the server.
    /// Receives and saves the file to the local directory.
    /// </summary>
    /// <param name="directory">The path of the file to download, relative to the server's base directory.</param>
    /// <returns>A task representing the asynchronous file download operation.</returns>
    public async Task<String?> GetCommandAsync(string directory)
    {   
        using TcpClient tcpClient = new TcpClient();
        await ConnectAsync(tcpClient);

        await using var stream = tcpClient.GetStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        await writer.WriteLineAsync($"2 {directory}");
        
        var response = await reader.ReadLineAsync();
        if (response == "-1")
        {
            return "File does not exist.";
        }
        if (!long.TryParse(response, out long contentSize))
        {
            throw new Exception($"Invalid response from server: {response}");
        }
        Console.WriteLine("responce:" + response);
        
        
        byte[] contentBytes = new byte[contentSize];
        int bytesRead = 0;
        Console.WriteLine(contentBytes.Length);
        
        while (bytesRead < contentSize)
        {
            int read = await stream.ReadAsync(contentBytes, bytesRead, (int)(contentSize - bytesRead));
    
            if (read == 0) 
            {
                throw new Exception("Unexpected end of stream or server closed the connection prematurely.");
            }
    
            bytesRead += read;
            Console.WriteLine($"Bytes read so far: {bytesRead}/{contentSize}");
        }
        
        return Encoding.UTF8.GetString(contentBytes);
    }
    
}