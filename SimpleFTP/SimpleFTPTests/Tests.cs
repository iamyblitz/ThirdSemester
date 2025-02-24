namespace SimpleFTPTests;

using SimpleFTP;
using NUnit.Framework;
using System.Text;
using System.IO;
using System.Threading.Tasks;
public class ServerClientTests
{
    private const int TestPort = 12345;
    private readonly string testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
    private Server _server;
    private Task _serverTask;
    private Client _client;

    [SetUp]
    public async Task Setup()
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
        await Task.Delay(100);
        _client = new Client("localhost", TestPort);
    }


    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(testDirectory))
        {
            DeleteDirectoryRecursively(testDirectory);
        }

        _server.StopServer();
    }

    private void DeleteDirectoryRecursively(string directoryPath)
    {
        foreach (var file in Directory.GetFiles(directoryPath))
        {
            File.Delete(file);
        }
        foreach (var directory in Directory.GetDirectories(directoryPath))
        {
            DeleteDirectoryRecursively(directory);
        }
        Directory.Delete(directoryPath);
    }

    [Test]
    public async Task TestDirectoryListing()
    {
        var response = await _client.ListCommandAsync("");
        var fileNames = response.Select(entry => entry.Name).ToList();
        Assert.IsTrue(fileNames.Contains("TestFile.txt"));
        Assert.IsTrue(fileNames.Contains("subDir1"));
        Assert.IsTrue(fileNames.Contains("subDir2"));
    }
    
    [Test]
    public async Task TestGetFile()
    {
        byte[] fileBytes = await _client.GetCommandAsync("TestFile.txt");
        string fileContent = Encoding.UTF8.GetString(fileBytes);
        Assert.AreEqual("This is a test file.", fileContent);
    }
}
