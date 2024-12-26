// <copyright file="MultiThreadLazy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// A lazy evaluation implementation for multithreaded environments.
/// Ensures that the value is computed only once, even in concurrent access scenarios.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MultiThreadLazy<T>: ILazy<T>
{
    private Func<T>? _supplier;
    private T _value = default;
    private volatile bool _isCalculated = false;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiThreadLazy{T}"/> class.
    /// </summary>
    /// <param name="supplier"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MultiThreadLazy(Func<T> supplier)
    {
        _supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    }

    /// <summary>
    ///  Returns the computed value. The computation occurs only on the first call.
    /// </summary>
    /// <returns></returns>
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
        return _value;
    }
}