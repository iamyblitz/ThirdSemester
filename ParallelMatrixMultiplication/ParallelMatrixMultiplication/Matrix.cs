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
namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// Represents an integer matrix.
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// Internal array storing matrix elements.
        /// </summary>
        private readonly int[,] _matrixArray;

        /// <summary>
        /// Initializes a matrix from a file.
        /// </summary>
        /// <param name="filepath">Path to the file with matrix data.</param>
        /// <exception cref="ArgumentException">Thrown if file is empty or data is invalid.</exception>
        public Matrix(string filepath)
        {
            _matrixArray = ConvertFileToMatrix(filepath);
        }

        /// <summary>
        /// Initializes a matrix with specified rows and columns.
        /// </summary>
        /// <param name="numOfRows">Number of rows.</param>
        /// <param name="numOfCols">Number of columns.</param>
        public Matrix(int numOfRows, int numOfCols)
        {
            _matrixArray = new int[numOfRows, numOfCols];
        }

        /// <summary>
        /// Initializes a matrix with a given array.
        /// </summary>
        /// <param name="matrixArray">2D array of integers.</param>
        public Matrix(int[,] matrixArray)
        {
            _matrixArray = matrixArray;
        }

        /// <summary>
        /// Gets or sets an element at the specified row and column.
        /// </summary>
        /// <param name="_numOfRows">Row index.</param>
        /// <param name="_numOfCols">Column index.</param>
        public int this[int _numOfRows, int _numOfCols]
        {
            get => _matrixArray[_numOfRows, _numOfCols];
            set => _matrixArray[_numOfRows, _numOfCols] = value;
        }
        
        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        public int NumOfRows => _matrixArray.GetLength(0);

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int NumOfCols => _matrixArray.GetLength(1);
        
        /// <summary>
        /// Converts file data to a 2D array.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        /// <returns>A 2D integer array.</returns>
        /// <exception cref="ArgumentException">Thrown if file is empty or contains non-integer values.</exception>
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
                        throw new ArgumentException("Matrix elements must be integers.");
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Returns the transposed matrix.
        /// </summary>
        /// <returns>A new transposed matrix.</returns>
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

        /// <summary>
        /// Exception thrown for an invalid block size.
        /// </summary>
        public class InvalidBlockSizeException : Exception
        {
            /// <summary>
            /// Initializes the exception with a message.
            /// </summary>
            /// <param name="message">Error message.</param>
            public InvalidBlockSizeException(string message) : base(message)
            {
            }
        }
    }
}
