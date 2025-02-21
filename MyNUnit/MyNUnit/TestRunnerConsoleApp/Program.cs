using System;
using MyNUnit.TestRunnerLibrary.Core;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Укажите путь к сборкам.");
       
        

        var path = Console.ReadLine();
        var testRunner = new TestRunner();
        var results = testRunner.RunTests(path);

        var reportGenerator = new ReportGenerator();
        reportGenerator.PrintReport(results);
    }
}
