namespace ClientServerChat;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Class chat for client server interaction.
/// </summary>
public class Chat
{
    /// <summary>
    /// Method for choosing client or server
    /// </summary>
    /// <param name="command"></param>
    public static async Task Main(string[] command)
    {
        if (command.Length == 1)
        {
            int port = int.Parse(command[0]);
            Console.WriteLine("Using:");
            Console.WriteLine("  Server: ChatApp <port>");
            await RunServerAsync(port);
        }
        else if (command.Length == 2)
        {
            string ipAddress = command[0];
            int port = int.Parse(command[1]);
           
            Console.WriteLine("Using:");
            Console.WriteLine("  Client: ChatApp <ipAddress> <port>");
            await RunClientAsync(ipAddress, port);
        }
    }
    /// <summary>
    /// Method for  running a server
    /// </summary>
    /// <param name="port"></param>
    public static async Task RunServerAsync(int port)
    {
        using (TcpListener listener = new TcpListener(IPAddress.Any, port))
        {
            listener.Start();
            Console.WriteLine("Server. Waiting for a client...");

            using (TcpClient client = await listener.AcceptTcpClientAsync())
            {
                Console.WriteLine("Client connected.");

                using (NetworkStream stream = client.GetStream())
                {
                    Task receiveStream = ReceiveMessagesAsync(stream);
                    Task sendStream = SendMessagesAsync(stream);

                    await Task.WhenAll(receiveStream, sendStream);
                }
            }

            listener.Stop();
        }
    }
    /// <summary>
    /// Method for sending a message 
    /// </summary>
    /// <param name="stream"></param>
    public static async Task SendMessagesAsync(NetworkStream stream)
    {
        while (true)
        {
            string message = Console.ReadLine();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);

            if (message.ToLower() == "exit")
                break;
        }
    }
    /// <summary>
    /// Methos for receiving a message
    /// </summary>
    /// <param name="stream"></param>
    public static async Task ReceiveMessagesAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received message: " + message);

            if (message.ToLower() == "exit")
                break;
        }
    }
    /// <summary>
    /// Method for running a client
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    public static async Task RunClientAsync(string ipAddress, int port)
    {
        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync(ipAddress, port);
            Console.WriteLine("Client connected to the server.");

            using (NetworkStream stream = client.GetStream())
            {
                Task receiveStream = ReceiveMessagesAsync(stream);
                Task sendStream = SendMessagesAsync(stream);

                await Task.WhenAll(receiveStream, sendStream);
            }
        }
    }
}
