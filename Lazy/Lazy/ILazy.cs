namespace Lazy;
/// <summary>
/// Interface for lazy evaluation of a value.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ILazy<T>
{
    /// <summary>
    /// Returns the computed value. The computation occurs only on the first call.
    /// Subsequent calls return the same value.
    /// </summary>
    /// <returns></returns>
    T Get();
}