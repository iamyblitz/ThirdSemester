namespace MyThreadPool;
/// <summary>
/// Represents a task that can be executed by the thread pool.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public interface IMyTask<TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    public bool IsCompleted { get; }
    
    /// <summary>
    /// Gets the result of the task. Blocks if the task has not yet completed.
    /// </summary>
    /// <exception cref="AggregateException">Thrown if the task completed with an exception.</exception>
    public TResult Result{ get; }
    
    /// <summary>
    /// Creates a continuation task that will be started after this task completes.
    /// </summary>
    /// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
    /// <param name="continuation">The function to execute when this task completes.</param>
    /// <returns>A new task representing the continuation.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
}