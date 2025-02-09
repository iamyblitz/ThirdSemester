
namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// Provides methods for sequential and parallel matrix multiplication.
    /// </summary>
    public static class MatrixMultiplication
    {
        /// <summary>
        /// Performs sequential matrix multiplication.
        /// </summary>
        /// <param name="matrixA">The first matrix.</param>
        /// <param name="matrixB">The second matrix.</param>
        /// <returns>The resulting matrix after multiplication.</returns>
        public static Matrix SequentialMultiply(Matrix matrixA, Matrix matrixB)
        {
            int[,] matrixC = new int[matrixA.NumOfRows, matrixB.NumOfCols];
            for (int i = 0; i < matrixA.NumOfRows; i++)
            {
                for (int j = 0; j < matrixB.NumOfCols; j++)
                {
                    for (int k = 0; k < matrixA.NumOfCols; k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }

            return new Matrix(matrixC);
        }

        /// <summary>
        /// Performs parallel matrix multiplication using block decomposition.
        /// </summary>
        /// <param name="matrixA">The first matrix.</param>
        /// <param name="matrixB">The second matrix.</param>
        /// <returns>The resulting matrix after multiplication.</returns>
        public static Matrix ParallelBlockMatrixMultiplication(Matrix matrixA, Matrix matrixB)
        {
            ParallelMatrixInfo result = new ParallelMatrixInfo(matrixA, matrixB);
            return new Matrix(result.Multiply());
        }

        /// <summary>
        /// Helper class for parallel matrix multiplication using block-based decomposition.
        /// </summary>
        private class ParallelMatrixInfo
        {
            private Matrix _matrixA;
            private Matrix _matrixB;
            private int[,] _resultMatrix;
            private int _numOfThreads = Environment.ProcessorCount;
            private Thread[] threads;
            private readonly int _blockSizeRows;
            private readonly int _blockSizeCols;
            private readonly int _numRowBlocks;
            private readonly int _numColBlocks;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParallelMatrixInfo"/> class.
            /// </summary>
            /// <param name="matrixA">The first matrix.</param>
            /// <param name="matrixB">The second matrix.</param>
            /// <exception cref="InvalidOperationException">Thrown when the matrices have incompatible dimensions.</exception>
            public ParallelMatrixInfo(Matrix matrixA, Matrix matrixB)
            {
                if (matrixA.NumOfCols != matrixB.NumOfRows)
                {
                    throw new InvalidOperationException("Matrices dimensions are not compatible for multiplication.");
                }
                _matrixA = matrixA;
                _matrixB = matrixB;
                _resultMatrix = new int[matrixA.NumOfRows, matrixB.NumOfCols];

                _numRowBlocks = (int)Math.Floor(Math.Sqrt(_numOfThreads));
                if (_numRowBlocks < 1)
                    _numRowBlocks = 1;
                _numColBlocks = _numOfThreads / _numRowBlocks;

                _blockSizeRows = (int)Math.Ceiling((double)matrixA.NumOfRows / _numRowBlocks);
                _blockSizeCols = (int)Math.Ceiling((double)matrixB.NumOfCols / _numColBlocks);

                threads = new Thread[_numOfThreads];
            }

            /// <summary>
            /// Performs matrix multiplication using parallel processing with block decomposition.
            /// </summary>
            /// <returns>The resulting matrix as a 2D array.</returns>
            public int[,] Multiply()
            {
                int threadIndex = 0;

                for (int i = 0; i < _numRowBlocks; i++)
                {
                    int rowStart = i * _blockSizeRows;
                    for (int j = 0; j < _numColBlocks; j++)
                    {
                        int colStart = j * _blockSizeCols;

                        threads[threadIndex] = new Thread(() => MultiplyBlock(rowStart, colStart));
                        threads[threadIndex].Start();
                        threadIndex++;
                    }
                }

                for (int i = 0; i < threadIndex; i++)
                {
                    threads[i].Join();
                }

                return _resultMatrix;
            }

            /// <summary>
            /// Multiplies a specific block of the matrices.
            /// </summary>
            /// <param name="rowStart">The starting row index.</param>
            /// <param name="colStart">The starting column index.</param>
            private void MultiplyBlock(int rowStart, int colStart)
            {
                int rowEnd = Math.Min(rowStart + _blockSizeRows, _matrixA.NumOfRows);
                int colEnd = Math.Min(colStart + _blockSizeCols, _matrixB.NumOfCols);

                for (int i = rowStart; i < rowEnd; i++)
                {
                    for (int j = colStart; j < colEnd; j++)
                    {
                        for (int k = 0; k < _matrixA.NumOfCols; k++)
                        {
                            _resultMatrix[i, j] += _matrixA[i, k] * _matrixB[k, j];
                        }
                    }
                }
            }
        }
    }
}
