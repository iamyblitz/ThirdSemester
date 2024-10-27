namespace SimpleFTPTests;
using SimpleFTP;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

[TestFixture]
public class Tests
{
    private Server? _server;
    private Task? _serverTask;
    private const int TestPort = 8888;
    private readonly string testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");

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
        _serverTask = Task.Run(async () => await _server.StartAsync());
    }

    [TearDown]
    public void TearDown()
    {
        _server = null;
        _serverTask = null;
    }

    [Test]
    public async Task TestListCommandAsync()
    {
        using var client = new Client("127.0.0.1", TestPort);
        await client.ListCommand("");
        Assert.Pass();
    }

    [Test]
    public async Task TestGetCommandAsync_FileExists()
    {
        using var client = new Client("127.0.0.1", TestPort);
        string filePath = "subDir1/TestFile1.txt";
        string expectedFilePath = Path.GetFileName(filePath);
        
        await client.GetCommand(filePath);
        
        Assert.IsTrue(File.Exists(expectedFilePath));
        
        if (File.Exists(expectedFilePath))
        {
            File.Delete(expectedFilePath);
        }
    }

    [Test]
    public async Task TestGetCommandAsync_FileDoesNotExist()
    {
        using var client = new Client("127.0.0.1", TestPort);
        string nonExistentFilePath = "nonExistentFile.txt";
        
        try
        {
            await client.GetCommand(nonExistentFilePath);
            Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.IsTrue(ex.Message.Contains("File does not exist"));
        }
    }

    [Test]
    public async Task TestGetCommandAsync_FileContentMatches()
    {
        using var client = new Client("127.0.0.1", TestPort);
        string filePath = "subDir1/TestFile1.txt";
        string expectedFilePath = Path.GetFileName(filePath);
        string expectedContent = "This is a test file in subDir1.";
        
        await client.GetCommand(filePath);
        
        Assert.IsTrue(File.Exists(expectedFilePath));
        string actualContent = File.ReadAllText(expectedFilePath);
        Assert.That(actualContent, Is.EqualTo(expectedContent));
        
        if (File.Exists(expectedFilePath))
        {
            File.Delete(expectedFilePath);
        }
    }
}
