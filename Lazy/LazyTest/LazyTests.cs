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

