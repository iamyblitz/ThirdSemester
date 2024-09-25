namespace Lazy;

/// <summary>
/// Contains tests for single-threaded and multi-threaded lazy evaluation implementations.
/// Tests both correct and incorrect behaviors of lazy computation in various environments.
/// </summary>
[TestFixture]
public class SingleThreadLazyTests
{
    /// <summary>
    /// Tests that the value is computed only once in a single-threaded environment.
    /// Ensures that repeated calls to <see cref="SingleThreadLazy{T}.Get"/> return the same value without re-invoking the supplier.
    /// </summary>
    [Test]
    public void Get_ShouldComputeValueOnce()
    {
        int callCount = 0;
        Func<int> supplier = () =>
        {
            callCount++;
            return 42;
        };

        var lazy = new SingleThreadLazy<int>(supplier);
        
        int result1 = lazy.Get();
        int result2 = lazy.Get();
        
        Assert.AreEqual(42, result1);
        Assert.AreEqual(42, result2);
        Assert.AreEqual(1, callCount); 
    }

    /// <summary>
    /// Tests that <see cref="SingleThreadLazy{T}.Get"/> returns null when the supplier function returns null.
    /// Ensures that the lazy evaluation handles null values correctly.
    /// </summary>
    [Test]
    public void Get_ShouldReturnNull_WhenSupplierReturnsNull()
    {
        var lazy = new SingleThreadLazy<string?>(() => null);
        
        string? result = lazy.Get();
        
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that the constructor of <see cref="SingleThreadLazy{T}"/> throws an <see cref="ArgumentNullException"/> when the supplier is null.
    /// </summary>
    [Test]
    public void Constructor_ShouldThrowException_WhenSupplierIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SingleThreadLazy<int>(null!));
    }
}

/// <summary>
/// Contains tests for multi-threaded lazy evaluation implementations.
/// Ensures thread-safe lazy computation and tests for potential race conditions.
/// </summary>
[TestFixture]
public class MultiThreadLazyTests
{
    /// <summary>
    /// Tests that the value is computed only once in a multi-threaded environment.
    /// Ensures that the supplier function is invoked only once even when multiple threads call <see cref="MultiThreadLazy{T}.Get"/> concurrently.
    /// </summary>
    [Test]
    public void Get_ShouldComputeValueOnce_InMultithreadedEnvironment()
    {
        int callCount = 0;
        object locker = new object();
        
        Func<int> supplier = () =>
        {
            lock (locker)  
            {
                callCount++;
            }
            return 42;
        };

        var lazy = new MultiThreadLazy<int>(supplier);
        int[] results = new int[10];
        Thread[] threads = new Thread[10];
        
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                int result = lazy.Get();
                results[i] = result;
            });
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        foreach (var result in results)
        {
            Assert.AreEqual(42, result); 
        }

        Assert.AreEqual(1, callCount);  
    }

    /// <summary>
    /// Tests that <see cref="MultiThreadLazy{T}.Get"/> correctly handles null values in a multi-threaded environment.
    /// Ensures that all threads receive a null result when the supplier returns null.
    /// </summary>
    [Test]
    public void Get_ShouldHandleNullSupplierResult_InMultithreadedEnvironment()
    {
        var lazy = new MultiThreadLazy<string?>(() => null);
        string?[] results = new string?[10];
        Thread[] threads = new Thread[10];
        
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                string? result = lazy.Get();
                results[i] = result;
            });
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        foreach (var result in results)
        {
            Assert.IsNull(result);  
        }
    }

    /// <summary>
    /// Tests that the constructor of <see cref="MultiThreadLazy{T}"/> throws an <see cref="ArgumentNullException"/> when the supplier is null.
    /// </summary>
    [Test]
    public void Constructor_ShouldThrowException_WhenSupplierIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MultiThreadLazy<int>(null!));
    }
}
