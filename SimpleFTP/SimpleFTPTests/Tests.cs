using NUnit.Framework;
using SimpleFTP;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFTPTests
{
    [TestFixture]
    public class Tests
    {
        private Server server;
        private CancellationTokenSource cancellationTokenSource;
        private const int TestPort = 8888;
        private readonly string testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");

        [SetUp]
        public async Task Setup()
        {
            // Create test directory and files
            if (!Directory.Exists(testDirectory))
            {
                Directory.CreateDirectory(testDirectory);
                Directory.CreateDirectory(Path.Combine(testDirectory, "subDir1"));
                Directory.CreateDirectory(Path.Combine(testDirectory, "subDir2"));
                File.WriteAllText(Path.Combine(testDirectory, "TestFile.txt"), "This is a test file.");
                File.WriteAllText(Path.Combine(testDirectory, "subDir1", "TestFile1.txt"), "Test file in subDir1.");
                File.WriteAllText(Path.Combine(testDirectory, "subDir2", "TestFile2.txt"), "Test file in subDir2.");
            }

            // Start server
            cancellationTokenSource = new CancellationTokenSource();
            server = new Server(TestPort, testDirectory);
            _ = Task.Run(() => server.StartAsync(), cancellationTokenSource.Token);
            await Task.Delay(500);
        }

        [TearDown]
        public void TearDown()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        [Test]
        public async Task TestListCommandAsync()
        {
            using var client = new Client("127.0.0.1", TestPort);
            var entries = await client.ListCommand("");
            Assert.IsNotNull(entries);
            Assert.IsNotEmpty(entries);
            Console.WriteLine("ListCommand output:");
            foreach (var entry in entries)
            {
                Console.WriteLine($"{entry.name} {(entry.isDirectory ? "<DIR>" : "<FILE>")}");
            }
        }

        [Test]
        public async Task TestGetCommandAsync_FileExists()
        {
            using var client = new Client("127.0.0.1", TestPort);
            string filePath = Path.Combine("subDir1", "TestFile1.txt");

            await client.GetCommand(filePath);
            string fileName = Path.GetFileName(filePath);
            Assert.IsTrue(File.Exists(fileName), $"Expected file '{fileName}' to be downloaded.");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        [Test]
        public async Task TestGetCommandAsync_FileDoesNotExist()
        {
            using var client = new Client("127.0.0.1", TestPort);
            await client.GetCommand("nonExistentFile.txt");
            Assert.Pass();
        }

        [Test]
        public async Task TestGetCommandAsync_FileContentMatches()
        {
            using var client = new Client("127.0.0.1", TestPort);
            string filePath = Path.Combine("subDir1", "TestFile1.txt");
            string expectedContent = "Test file in subDir1.";

            await client.GetCommand(filePath);

            string fileName = Path.GetFileName(filePath);
            Assert.IsTrue(File.Exists(fileName), $"Downloaded file '{fileName}' does not exist.");
            string actualContent = await File.ReadAllTextAsync(fileName);
            Assert.That(actualContent, Is.EqualTo(expectedContent), "File content does not match.");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}
