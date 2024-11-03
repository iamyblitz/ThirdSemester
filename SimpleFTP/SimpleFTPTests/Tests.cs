namespace SimpleFTPTests;
using SimpleFTP;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


public class ServerClientTests
{
    private const int TestPort = 12345;
    private readonly string testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
    private Server _server;
    private Task _serverTask;
    private Client _client;

    [SetUp]
    public void Setup()
    {
        if (!Directory.Exists(testDirectory))
        {
            Directory.CreateDirectory(testDirectory);
            Directory.CreateDirectory(Path.Combine(testDirectory, "subDir1"));
            Directory.CreateDirectory(Path.Combine(testDirectory, "subDir1", "subDir11"));
            Directory.CreateDirectory(Path.Combine(testDirectory, "subDir2"));
            File.WriteAllText(Path.Combine(testDirectory, "TestFile.txt"), "This is a test file.");
            File.WriteAllText(Path.Combine(testDirectory, "subDir1", "TestFile1.txt"), "This is a test file in subDir1.");
            File.WriteAllText(Path.Combine(testDirectory, "subDir2", "TestFile2.txt"), "This is a test file in subDir2.");
        }

       
        _server = new Server(TestPort, testDirectory);
        _serverTask = Task.Run(() => _server.StartAsync());
       
        _client = new Client("localhost", TestPort);
    }

    [TearDown]
    public void TearDown()
    {
        _server.Stop();
        _serverTask.Wait();
        _client.Dispose();
    }

    [Test]
    public async Task TestDirectoryListing()
    {
        string response = await _client.ListCommandAsync(""); 
        Assert.IsTrue(response.Contains("TestFile.txt"));
        Assert.IsTrue(response.Contains("subDir1"));
        Assert.IsTrue(response.Contains("subDir2"));
    }

    [Test]
    public async Task TestFileRetrieval()
    {
        string response = await _client.GetCommandAsync("TestFile.txt"); // Relative path
        Assert.IsTrue(response.Contains("This is a test file."));
    }

    [Test]
    public async Task TestInvalidDirectory()
    {
        string response = await _client.ListCommandAsync("NonExistentDir");
        Assert.AreEqual("Directory doesn't exist.", response);
    }

    [Test]
    public async Task TestInvalidFile()
    {
        string response = await _client.GetCommandAsync("NonExistentFile.txt");
        Assert.AreEqual("File does not exist.", response);
    }

}
