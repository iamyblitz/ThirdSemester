namespace ParallelMatrixMultiplication;

public class Matrix
{
    private readonly int[,] _matrixArray;
    private readonly int _numOfRows;
    private readonly int _numOfCols;

    //constructor
    public Matrix(string filepath)
    {
        _matrixArray = convertFileToMatrix(filepath);
        _numOfRows = _matrixArray.GetLength(0);
        _numOfCols = _matrixArray.GetLength(1);
        
    }
    
    public Matrix(int numOfRows, int numOfCols)
    {
        _matrixArray = new int[numOfRows, numOfCols];
        _numOfRows = numOfRows;
        _numOfCols = numOfCols;
        
    }
    
    //indexer
    public int this[int _numOfRows, int _numOfCols ]
    {
        get { return _matrixArray[_numOfRows, _numOfCols]; }
        set { _matrixArray[_numOfRows, _numOfCols] = value; }
    }
    
    //properties
    public int NumOfRows => _numOfRows;
    public int NumOfCols => _numOfCols;
    
    private int[,] convertFileToMatrix(string filepath)
    {
       
        string[] lines = File.ReadAllLines(filepath);

        if (lines.Length == 0)
        {
            throw new ArgumentException("The file path is empty.");
        }
        
       
        int numOfRows = lines.Length;
        int numOfCols = lines[0].Split(' ').Length;

        
        int[,] matrix = new int[numOfRows, numOfCols];

        
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
        Matrix transMatrix = new Matrix(_numOfRows, _numOfCols);
        for (int i = 0; i < _numOfRows; i++)
        {
            for (int j = 0; j < _numOfCols; j++)
            {
                transMatrix[j, i] = _matrixArray[i, j];
            }
        }

        return transMatrix;
        
    }
}