namespace SimpleFTP;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

public class Client(string server, int port) : IDisposable
{
    private readonly TcpClient tcpClient = new();

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
    
    public async Task ListCommand(string directory)
    {
        await ConnectAsync();

        await using var stream = tcpClient.GetStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.AutoFlush = true;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        string request = $"1 {directory}\n";
        await writer.WriteLineAsync(request);

        string? response = await reader.ReadLineAsync();
        
        if (response == "size -1")
        {
            Console.WriteLine("Directory doesn't exist.");
        }
        else
        {
            Console.WriteLine("Files and directories:");
            Console.WriteLine(response);
            while (!string.IsNullOrEmpty(response = await reader.ReadLineAsync()))
            {
                Console.WriteLine(response);
            }
        }
    }
    
    public async Task GetCommand(string directory)
    {
        await ConnectAsync();

        await using var stream = tcpClient.GetStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.AutoFlush = true;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        await writer.WriteLineAsync($"2 {directory}\n");
        
        var response = await reader.ReadLineAsync();
        if (response == "size -1")
        {
            Console.WriteLine("File does not exist.");
        }
        else
        {
            var responseInfo = response?.Split(' ');
            if (responseInfo[0] == "size")
            {
                long size = long.Parse(responseInfo[1]);
                byte[] buffer = new byte[size];

                var readAsync = await stream.ReadAsync(buffer, 0, (int)size);
                string fileName = Path.GetFileName(directory);
                
                await File.WriteAllBytesAsync(fileName, buffer);
                Console.WriteLine($"File '{fileName}' has been downloaded ({size} bytes).");
            }
            else
            {
                Console.WriteLine("Unexpected response format.");
            }
        }
    }
    
    public void Dispose()
    {
        tcpClient?.Close();
    }
    
}