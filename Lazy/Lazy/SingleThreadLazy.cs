// <copyright file="SingleThreadLazy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// A lazy evaluation implementation for single-threaded environments.
/// The value is computed only once during the first call to Get.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingleThreadLazy<T>: ILazy<T>
{
    private Func<T>? _supplier;
    private T _value = default;
    private bool _isCalculated = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleThreadLazy{T}"/> class.
    /// </summary>
    /// <param name="supplier"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SingleThreadLazy(Func<T> supplier)
        => _supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));

    /// <summary>
    /// Returns the computed value. The computation occurs only on the first call.
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        if (!_isCalculated)
        {
            _value = _supplier();
            _isCalculated = true;
            _supplier = null;
        }
        return _value;
    }
}