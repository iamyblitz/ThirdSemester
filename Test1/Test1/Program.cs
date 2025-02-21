
using System.Diagnostics;
namespace Test1;

class Program
{
    static void Main(string[] args)
    {
        string path;
        Console.Write("Введите полный путь к директории: ");
        path = Console.ReadLine();

        var checkSum = new CheckSumDirectory();
        
        var stopwatchSingle = Stopwatch.StartNew();
        byte[] singleThreadHash = checkSum.CheckSumSingleThread(path);
        stopwatchSingle.Stop();

        
        var stopwatchMulti = Stopwatch.StartNew();
        byte[] multiThreadHash = checkSum.CheckSumMultiThread(path);
        stopwatchMulti.Stop();
        

        Console.WriteLine("Однопоточный метод:");
        
        Console.WriteLine($"Время выполнения: {stopwatchSingle.ElapsedMilliseconds} мс\n");

        Console.WriteLine("Многопоточный метод:");
       
        Console.WriteLine($"Время выполнения: {stopwatchMulti.ElapsedMilliseconds} мс\n");
    }
}

