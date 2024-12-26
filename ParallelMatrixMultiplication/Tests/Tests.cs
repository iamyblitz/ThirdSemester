namespace ParallelMatrixMultiplication;

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

public class MatrixMultiplicationTests
{
    const string testDataPath = "ParallelMatrixMultiplication/Tests/testData/";

    private string testSmallMatrixAPath =  "ParallelMatrixMultiplication/Tests/testData/matrix02_50_50.txt";
    private string testSmallMatrixBPath = "ParallelMatrixMultiplication/Tests/testData/matrix022_50_50.txt";

    private string testBigMatrixAPath = testDataPath + "matrix01_500_500.txt";
    private string testBigMatrixBPath = testDataPath + "matrix02_500_500.txt";

    private string testUnevenMatrixAPath = testDataPath + "matrix03_500_667.txt";
    private string testUnevenMatrixBPath = testDataPath + "matrix03_667_500.txt";

    public void SequentialAndParallelMultiplication_ShouldReturnSameResult(string testMatrixAPath,string testMatrixBPath)
    {
        Matrix matrixA = new Matrix(testMatrixAPath);
        Matrix matrixB = new Matrix(testMatrixBPath);

        Matrix sequentialResult = MatrixMultiplication.SequentialMultiply(matrixA, matrixB);
        Matrix parallelResult = MatrixMultiplication.ParallelBlockMatrixMultiplication(matrixA, matrixB);

        for (int i = 0; i < sequentialResult.NumOfRows; i++)
        {
            for (int j = 0; j < sequentialResult.NumOfCols; j++)
            {
                    Assert.That(parallelResult[i, j], Is.EqualTo(sequentialResult[i, j]), 
                        $"Matrices differ at element [{i},{j}]");
            }
        }
    }

    [Test]
    public void TestLittleMatrixMultiplication()
    {
        SequentialAndParallelMultiplication_ShouldReturnSameResult(testSmallMatrixAPath, testSmallMatrixBPath);
    }

    [Test]
    public void TestBigMatrixMultiplication()
    {
        SequentialAndParallelMultiplication_ShouldReturnSameResult(testBigMatrixAPath, testBigMatrixBPath);
    }

    [Test]
    public void TestUnevenMatrixMultiplication()
    {
        SequentialAndParallelMultiplication_ShouldReturnSameResult(testUnevenMatrixAPath, testUnevenMatrixBPath);
    }
}

