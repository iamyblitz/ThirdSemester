namespace Lazy;

public class MultiThreadLazy<T>: ILazy<T>
{
    private Func<T>? _supplier;
    private T? _value = default;
    private bool _isCalculated = false;
    private readonly object _lockObject = new object();

    public MultiThreadLazy(Func<T> supplier)
    {
        _supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    }

    public T Get()
    {
        if (_isCalculated)
        {
            return _value;
        }
        
        lock (_lockObject)
        {
            if (!_isCalculated)
            {
                _value = _supplier();
                _isCalculated = true;
                _supplier = null;
            }
        }
        
        return _value!;
    }
}