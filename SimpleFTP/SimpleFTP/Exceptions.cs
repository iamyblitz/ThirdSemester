// <copyright file="Exceptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFTP;

/// <summary>
/// Base exception for FTP client errors.
/// </summary>
public class FtpException : Exception
{
    public FtpException(string message) : base(message) { }
}

/// <summary>
/// Exception for handling missing directories.
/// </summary>
public class DirectoryNotFoundException : FtpException
{
    public DirectoryNotFoundException(string path)
        : base($"Directory does not exist: {path}") { }
}

/// <summary>
/// Exception for handling missing files.
/// </summary>
public class FileNotFoundException : FtpException
{
    public FileNotFoundException(string path)
        : base($"File does not exist: {path}") { }
}

/// <summary>
/// Exception for invalid responses from the server.
/// </summary>
public class InvalidResponseException : FtpException
{
    public InvalidResponseException(string response)
        : base($"Invalid response from server: {response}") { }
}


/// <summary>
/// Exception for connection failures.
/// </summary>
public class ConnectionFailedException : FtpException
{
    public ConnectionFailedException(string server, int port)
        : base($"Failed to connect to {server}:{port}. ") { }
}
