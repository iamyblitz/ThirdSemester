namespace ParallelMatrixMultiplication;

public class MatrixMultiplication
{
    public static int[,] SequentialMultiply(Matrix matrixA, Matrix matrixB)
    {
        int[,] matrixC = new int[matrixA.NumOfRows, matrixB.NumOfCols];
        for (int i = 0; i < matrixA.NumOfRows; i++)
        {
            for (int j = 0; j < matrixB.NumOfCols; j++)
            {
                matrixC[i, j] = 0;
                for (int k = 0; k < matrixA.NumOfCols; k++)
                {
                    matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }
        }
        return matrixC;
    }

    public static int[,] ParallelBlockMatrixMultiplication(Matrix matrixA, Matrix matrixB)
    {
        ParallelMatrixInfo result = new ParallelMatrixInfo(matrixA, matrixB);
        return result.Multiply();
    }

    private class ParallelMatrixInfo
    {
        private Matrix _matrixA;
        private Matrix _matrixB;
        private int[,] _resultMatrix;
        private int _numOfThreads = 16; 
        private Thread[] threads;
        private readonly int _blockSizeRows;
        private readonly int _blockSizeCols;

        public ParallelMatrixInfo(Matrix matrixA, Matrix matrixB)
        {
            if (matrixA.NumOfCols != matrixB.NumOfRows)
                throw new InvalidOperationException("Matrices dimensions are not compatible for multiplication.");

            _matrixA = matrixA;
            _matrixB = matrixB;
            _resultMatrix = new int[matrixA.NumOfRows, matrixB.NumOfCols];

           
            _blockSizeRows = (int)Math.Ceiling((double)matrixA.NumOfRows / Math.Sqrt(_numOfThreads));
            _blockSizeCols = (int)Math.Ceiling((double)matrixB.NumOfCols / Math.Sqrt(_numOfThreads));

            threads = new Thread[_numOfThreads];
        }
        
        public int[,] Multiply()
        {
            int threadIndex = 0;
            for (int i = 0; i < _matrixA.NumOfRows; i += _blockSizeRows)
            {
                for (int j = 0; j < _matrixB.NumOfCols; j += _blockSizeCols)
                {
                    if (threadIndex >= _numOfThreads)
                        throw new Exception("More blocks than threads. Adjust your block size or number of threads.");

                  
                    int blockRowStart = i;
                    int blockColStart = j;
                    threads[threadIndex] = new Thread(() => MultiplyBlock(blockRowStart, blockColStart));
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