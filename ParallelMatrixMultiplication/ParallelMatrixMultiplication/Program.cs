namespace ParallelMatrixMultiplication;
using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string testMatrixAPath = "matrixA.txt";
        string testMatrixBPath = "matrixB.txt";
        string testResultsPath = "results.txt";

        var timeMeasurement = new TimeMeasurement();

        timeMeasurement.SequentialMultiplication_TimeMeasurement(testMatrixAPath, testMatrixBPath, testResultsPath);

        timeMeasurement.ParallelMultiplication_TimeMeasurement(testMatrixAPath, testMatrixBPath, testResultsPath);
    }
}

public class TimeMeasurement
{
    public void SequentialMultiplication_TimeMeasurement(string testMatrixAPath, string testMatrixBPath, string testResultsPath)
    {
        Matrix matrixA = new Matrix(testMatrixAPath);
        Matrix matrixB = new Matrix(testMatrixBPath);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Matrix result = MatrixMultiplication.SequentialMultiply(matrixA, matrixB);

        stopwatch.Stop();
        TimeSpan sequentialTime = stopwatch.Elapsed;

        string logPath = testResultsPath + "sequential_times.txt";
        File.AppendAllText(logPath, $"Sequential execution time: {sequentialTime.TotalMilliseconds} ms{Environment.NewLine}");
    }

    public void ParallelMultiplication_TimeMeasurement(string testMatrixAPath, string testMatrixBPath, string testResultsPath)
    {
        Matrix matrixA = new Matrix(testMatrixAPath);
        Matrix matrixB = new Matrix(testMatrixBPath);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Matrix result = MatrixMultiplication.ParallelBlockMatrixMultiplication(matrixA, matrixB);

        stopwatch.Stop();
        TimeSpan parallelTime = stopwatch.Elapsed;

        string logPath = testResultsPath + "parallel_times.txt";
        File.AppendAllText(logPath, $"Parallel execution time: {parallelTime.TotalMilliseconds} ms{Environment.NewLine}");
    }
}