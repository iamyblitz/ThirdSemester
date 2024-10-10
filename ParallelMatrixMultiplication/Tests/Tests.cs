namespace ParallelMatrixMultiplication;

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

public class MatrixMultiplicationTest
{
      [TestFixture]
    public class MatrixMultiplicationTests
    {
        const string testDataPath = "../../../Tests/testData/";
        const string testResultsPath = "../../../Tests/testResult/";
        
        private string testSmallMatrixAPath = testDataPath + "matrix02_50*50";
        private string testSmallMatrixBPath = testDataPath + "matrix022_50*50";
        
        private string testBigMatrixAPath = testDataPath + "matrix01_500*500";
        private string testBigMatrixBPath = testDataPath + "matrix02_500*500";
        
        private string testUnevenMatrixAPath = testDataPath + "matrix03_500*667";
        private string testUnevenMatrixBPath = testDataPath + "matrix03_667*500";
        

        public void SequentialAndParallelMultiplication_ShouldReturnSameResult(string testMatrixAPath,string testMatrixBPath)
        {
            Matrix matrixA = new Matrix(testMatrixAPath);
            Matrix matrixB = new Matrix(testMatrixBPath);
            
            int[,] sequentialResult = MatrixMultiplication.SequentialMultiply(matrixA, matrixB);
            int[,] parallelResult = MatrixMultiplication.ParallelBlockMatrixMultiplication(matrixA, matrixB);
            
            for (int i = 0; i < sequentialResult.GetLength(0); i++)
            {
                for (int j = 0; j < sequentialResult.GetLength(1); j++)
                {
                        Assert.That(parallelResult[i, j], Is.EqualTo(sequentialResult[i, j]), 
                            $"Matrices differ at element [{i},{j}]");
                }
            }
        }

        public void SequentialMultiplication_TimeMeasurement(string testMatrixAPath,string testMatrixBPath)
        {
            Matrix matrixA = new Matrix(testMatrixAPath);
            Matrix matrixB = new Matrix(testMatrixBPath);
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[,] result = MatrixMultiplication.SequentialMultiply(matrixA, matrixB);

            stopwatch.Stop();
            TimeSpan sequentialTime = stopwatch.Elapsed;
            
            string logPath = testResultsPath + "sequential_times.txt";
            File.AppendAllText(logPath, $"Sequential execution time: {sequentialTime.TotalMilliseconds} ms{Environment.NewLine}");
        }

        public void ParallelMultiplication_TimeMeasurement(string testMatrixAPath,string testMatrixBPath)
        {
            Matrix matrixA = new Matrix(testMatrixAPath);
            Matrix matrixB = new Matrix(testMatrixBPath);
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[,] result = MatrixMultiplication.ParallelBlockMatrixMultiplication(matrixA, matrixB);

            stopwatch.Stop();
            TimeSpan parallelTime = stopwatch.Elapsed;
            
            string logPath = testResultsPath + "parallel_times.txt";
            File.AppendAllText(logPath, $"Parallel execution time: {parallelTime.TotalMilliseconds} ms{Environment.NewLine}");
        }


        [Test]
        public void TestLittleMatrixMultiplication()
        {
            SequentialMultiplication_TimeMeasurement(testSmallMatrixAPath, testSmallMatrixBPath);
            SequentialAndParallelMultiplication_ShouldReturnSameResult(testSmallMatrixAPath, testSmallMatrixBPath);
            ParallelMultiplication_TimeMeasurement(testSmallMatrixAPath, testSmallMatrixBPath);
        }
        
        [Test]
        public void TestBigMatrixMultiplication()
        {
            SequentialMultiplication_TimeMeasurement(testBigMatrixAPath, testBigMatrixBPath);
            SequentialAndParallelMultiplication_ShouldReturnSameResult(testBigMatrixAPath, testBigMatrixBPath);
            ParallelMultiplication_TimeMeasurement(testBigMatrixAPath, testBigMatrixBPath);
        }
        
        [Test]
        public void TestUnevenMatrixMultiplication()
        {
            SequentialMultiplication_TimeMeasurement(testUnevenMatrixAPath, testUnevenMatrixBPath);
            SequentialAndParallelMultiplication_ShouldReturnSameResult(testUnevenMatrixAPath, testUnevenMatrixBPath);
            ParallelMultiplication_TimeMeasurement(testUnevenMatrixAPath, testUnevenMatrixBPath);
        }
    }
    
}