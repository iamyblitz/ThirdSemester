namespace Lazy;

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
        var results = new int[10];
        var threads = new Thread[10];

        for (int i = 0; i < threads.Length; i++)
        {
            int localIndex = i;
            threads[i] = new Thread(() =>
            {
                int result = lazy.Get();
                results[localIndex] = result;
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
