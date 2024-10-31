namespace Test1;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// class for checking sum of directory
/// </summary>
public class CheckSumDirectory
{
    /// <summary>
    /// Single thread method for checking sum
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
   public byte[] CheckSumSingleThread(string path)
   {
      if (!Directory.Exists(path))
      {
         throw new DirectoryNotFoundException($"Directory {path} does not exist");
      }

      return GetFileHashSingleThread(path);
   }
   
    /// <summary>
    /// Multi thread method for checking sum
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
   public byte[] CheckSumMultiThread(string path)
   {
      if (!Directory.Exists(path))
      {
         throw new DirectoryNotFoundException($"Directory {path} does not exist");
      }

      return GetFileHashMultiThread(path);
   }
   /// <summary>
   /// All the logic for calculatinh hash for path single thread
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   private byte[] GetFileHashSingleThread(string path)
   {
      if (!File.Exists(path) && !Directory.Exists(path))
      {
         throw new InvalidOperationException("Directory or File doesn't exists.");
      }
      
      using var md5 = MD5.Create();
      var bytesList = new List<byte>();
      
      var nameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(path));
      bytesList.AddRange(md5.ComputeHash(nameBytes));

      var filePaths = Directory.GetFiles(path);
      Array.Sort(filePaths);
      var dirPaths = Directory.GetDirectories(path);
      Array.Sort(dirPaths);

      foreach (var file in filePaths)
      {
         bytesList.AddRange(GetFileHash(file));
      }
      
      foreach (var dir in dirPaths)
      {
         bytesList.AddRange(GetFileHashSingleThread(dir));
      }
      
      return md5.ComputeHash(bytesList.ToArray());
   }

   /// <summary>
   /// Gets cash Of one file
   /// </summary>
   /// <param name="filePath"></param>
   /// <returns></returns>
   private byte[] GetFileHash(string filePath)
   {
      using var md5 = MD5.Create();
      using var stream = File.OpenRead(filePath);
      return md5.ComputeHash(stream);
   }
   
   /// <summary>
   /// All the logic for calculatinh hash for path milti thread
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   private byte[] GetFileHashMultiThread(string path)
   {
       if (!File.Exists(path) && !Directory.Exists(path))
       {
           throw new InvalidOperationException("Path doesn't exist.");
       }

       using var md5 = MD5.Create();
       using var memoryStream = new MemoryStream();

       var nameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(path));
       var nameHash = md5.ComputeHash(nameBytes);
       memoryStream.Write(nameHash, 0, nameHash.Length);

       if (Directory.Exists(path))
       {
           var filePaths = Directory.GetFiles(path);
           Array.Sort(filePaths);
           var dirPaths = Directory.GetDirectories(path);
           Array.Sort(dirPaths);

           var fileHashes = new List<byte[]>();
           var dirHashes = new List<byte[]>();


           Parallel.ForEach(filePaths, filePath =>
           {
               var hash = GetFileHash(filePath);
               lock (fileHashes)
               {
                   fileHashes.Add(hash);
               }
           });

           Parallel.ForEach(dirPaths, dirPath =>
           {
               var hash = GetFileHashMultiThread(dirPath);
               lock (dirHashes)
               {
                   dirHashes.Add(hash);
               }
           });

           Array.Sort(fileHashes.ToArray());
           Array.Sort(dirHashes.ToArray());

           foreach (var fileHash in fileHashes)
           {
               memoryStream.Write(fileHash, 0, fileHash.Length);
           }

           foreach (var dirHash in dirHashes)
           {
               memoryStream.Write(dirHash, 0, dirHash.Length);
           }
       }

       return md5.ComputeHash(memoryStream.ToArray());
   }
}
