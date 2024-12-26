/*
 * Program Name: [ParallelMatrixMultiplication]
 * Author: [Yana]
 * License: MIT
 * Copyright (c) [2024] [Yana]
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software to deal in the Software without restriction, subject to
 * the MIT License. See LICENSE file for details.
 */
namespace ParallelMatrixMultiplication;

public class Matrix
{
    private readonly int[,] _matrixArray;

    public Matrix(string filepath)
    {
        _matrixArray = ConvertFileToMatrix(filepath);
    }

    public Matrix(int numOfRows, int numOfCols)
    {
        _matrixArray = new int[numOfRows, numOfCols];
    }

    public Matrix(int[,] matrixArray)
    {
        _matrixArray = matrixArray;
    }

    //indexer
    public int this[int _numOfRows, int _numOfCols ]
    {
        get => _matrixArray[_numOfRows, _numOfCols];
        set => _matrixArray[_numOfRows, _numOfCols] = value;
    }
    
    //properties
    public int NumOfRows => _matrixArray.GetLength(0);
    public int NumOfCols => _matrixArray.GetLength(1);
    
    private int[,] ConvertFileToMatrix(string filepath)
    {
        string[] lines = File.ReadAllLines(filepath);

        if (lines.Length == 0)
        {
            throw new ArgumentException("The file is empty."); 
        }
        int numOfRows = lines.Length;
        int numOfCols = lines[0].Split(' ').Length;
        var matrix = new int[numOfRows, numOfCols];
        for (int i = 0; i < numOfRows; i++)
        {
            string[] elements = lines[i].Split(' ');

            for (int j = 0; j < numOfCols; j++)
            {
                bool result = int.TryParse(elements[j], out matrix[i, j]);

                if (!result)
                {
                    throw new ArgumentException("Matrix elements must be integer numbers.");
                }
            }
        }

        return matrix;
    }

    internal Matrix TransposeMatrix()
    {
        Matrix transposeMatrix = new(NumOfRows, NumOfCols);
        for (int i = 0; i < NumOfRows; i++)
        {
            for (int j = 0; j < NumOfCols; j++)
            {
                transposeMatrix[j, i] = _matrixArray[i, j];
            }
        }
        return transposeMatrix;
    }

    public class InvalidBlockSizeException : Exception
    {
        public InvalidBlockSizeException(string message) : base(message)
        {
        }
    }
}