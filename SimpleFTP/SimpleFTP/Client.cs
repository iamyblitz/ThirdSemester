// <copyright file="Client.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFTP
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Text;

    /// <summary>
    /// Represents a client that connects to an FTP-like server to list directories and download files over TCP.
    /// </summary>
    public class Client
    {
        private readonly string server;
        private readonly int port;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        public Client(string server, int port)
        {
            this.server = server;
            this.port = port;
        }

        /// <summary>
        /// Establishes a connection to the server asynchronously if not already connected.
        /// </summary>
        private async Task ConnectAsync(TcpClient tcpClient)
        {
            if (tcpClient.Connected) return;
            if (!tcpClient.Connected)
            {
                try
                {
                    await tcpClient.ConnectAsync(server, port);
                }
                catch (SocketException ex)
                {
                    throw new ConnectionFailedException(server, port);
                }
            }
        }

        /// <summary>
        /// Sends a request to list the contents of a directory on the server.
        /// Receives and displays the list of files and directories.
        /// </summary>
        public async Task<List<FileSystemEntry>> ListCommandAsync(string directory)
        {
            using TcpClient tcpClient = new();
            await ConnectAsync(tcpClient);

            await using var stream = tcpClient.GetStream();
            await using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string request = $"1 {directory}";
            await writer.WriteLineAsync(request);

            string? response = await reader.ReadLineAsync();

            if (response == "-1")
            {
                throw new DirectoryNotFoundException(directory);
            }

            int.TryParse(response, out int entriesCount);
            var resultList = new List<FileSystemEntry>();

            for (int i = 0; i < entriesCount; i++)
            {
                string? entryLine = await reader.ReadLineAsync();
                if (entryLine != null)
                {
                    var parts = entryLine.Split(' ');
                    if (parts.Length == 2 && bool.TryParse(parts[1], out bool isDirectory))
                    {
                        resultList.Add(new FileSystemEntry(parts[0], isDirectory));
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        /// Sends a request to download a file from the server.
        /// Receives and saves the file to the local directory.
        /// </summary>
        public async Task<byte[]> GetCommandAsync(string directory)
        {
            using TcpClient tcpClient = new ();
            await ConnectAsync(tcpClient);
            await using var stream = tcpClient.GetStream();
            await using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync($"2 {directory}");
            string response = await ReadLineFromStreamAsync(stream);

            if (response == "-1")
            {
                throw new FileNotFoundException(directory);
            }

            if (!long.TryParse(response, out long contentSize))
            {
                throw new InvalidResponseException(response);
            }

            Console.WriteLine("response:" + response);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            using var memoryStream = new MemoryStream();

            while (totalBytesRead < contentSize)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    throw new FtpException("Unexpected end of stream or server closed the connection.");
                }

                await memoryStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Reads a line (until '\n') from the given NetworkStream using UTF8 encoding.
        /// </summary>
        private async Task<string> ReadLineFromStreamAsync(NetworkStream stream)
        {
            List<byte> byteList = new List<byte>();
            byte[] buffer = new byte[1];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, 1);
                if (bytesRead == 0)
                {
                    break;
                }

                if (buffer[0] == (byte)'\n')
                {
                    break;
                }

                byteList.Add(buffer[0]);
            }

            return Encoding.UTF8.GetString(byteList.ToArray()).TrimEnd('\r');
        }


        /// <summary>
        /// Represents a file or directory entry from the server.
        /// </summary>
        public class FileSystemEntry
        {
            /// <summary>
            /// Gets the name of the file or directory.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets a value indicating whether the entry is a directory.
            /// </summary>
            public bool IsDirectory { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileSystemEntry"/> class.
            /// </summary>
            public FileSystemEntry(string name, bool isDirectory)
            {
                Name = name;
                IsDirectory = isDirectory;
            }

            /// <summary>
            /// Returns a string representation of the file or directory entry.
            /// </summary>
            public override string ToString()
            {
                return $"{(IsDirectory ? "[DIR]" : "[FILE]")} {Name}";
            }
        }
    }
}