namespace ClientServerChat;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

public class Tests
{
    private const int Port = 8888;
    private const string IpAddress = "127.0.0.1";

    [Test]
    public async Task Server_AcceptsClientConnection()
    {
        var serverTask = Chat.RunServerAsync(Port);

        var client = new TcpClient();
        await client.ConnectAsync(IpAddress, Port);

        Assert.IsTrue(client.Connected);

        client.Close();
        await serverTask;
    }

   

    [Test]
    public async Task SendAndReceiveMessages()
    {
        var serverTask = Chat.RunServerAsync(Port);
        await Task.Delay(1000);
        var client = new TcpClient();
        await client.ConnectAsync(IpAddress, Port);
        var clientStream = client.GetStream();


        string message = "Hello, server!";
        byte[] data = Encoding.UTF8.GetBytes(message);
        await clientStream.WriteAsync(data, 0, data.Length);

        byte[] buffer = new byte[1024];
        int bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Assert.AreEqual("Received: " + message, receivedMessage.Trim());

        client.Close();
        await serverTask;
    }
    
}