using System;
using System.Collections.Generic;
using System.Threading;

namespace MyThreadPool;

/// <summary>
/// A thread pool that manages a fixed number of threads to execute submitted tasks.
/// </summary>
public class MyThreadPool
{
    private readonly Thread[] _threads;
    private readonly Queue<Action> _taskQueue = new Queue<Action>();
    private readonly object _lock = new object();
    private bool _isShutdown = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class with the specified number of threads.
    /// </summary>
    /// <param name="numThreads">The number of threads in the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numThreads"/> is less than or equal to zero.</exception>
    public MyThreadPool(int numThreads)
    {
        if (numThreads <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numThreads), "Number of threads must be greater than zero.");
        }

        _threads = new Thread[numThreads];
        for (int i = 0; i < numThreads; i++)
        {
            _threads[i] = new Thread(Work);
            _threads[i].Start();
        }
    }

    /// <summary>
    /// Represents a task that can be executed by the thread pool.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private bool _isCompleted;
        private TResult _result;
        private readonly Func<TResult> _task;
        private readonly object _taskLock = new object();
        private readonly List<Action> _continuations = new List<Action>();
        private Exception _exception;
        private MyThreadPool _threadPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
        /// </summary>
        /// <param name="task">The function representing the task.</param>
        public MyTask(Func<TResult> task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _isCompleted = false;
        }

        /// <summary>
        /// Sets the thread pool associated with this task.
        /// </summary>
        /// <param name="threadPool">The thread pool.</param>
        public void SetThreadPool(MyThreadPool threadPool)
        {
            _threadPool = threadPool ?? throw new ArgumentNullException(nameof(threadPool));
        }

        /// <summary>
        /// Executes the task and handles completion and continuations.
        /// </summary>
        public void Execute()
        {
            try
            {
                TResult res = _task.Invoke();
                lock (_taskLock)
                {
                    _result = res;
                    _isCompleted = true;
                    Monitor.PulseAll(_taskLock); 
                }
            }
            catch (Exception ex)
            {
                lock (_taskLock)
                {
                    _exception = ex;
                    _isCompleted = true;
                    Monitor.PulseAll(_taskLock); 
                }
            }

            ExecuteContinuations();
        }

        /// <summary>
        /// Executes any continuations added to this task.
        /// </summary>
        private void ExecuteContinuations()
        {
            lock (_taskLock)
            {
                foreach (var continuation in _continuations)
                {
                    _threadPool.EnqueueTask(continuation);
                }
                _continuations.Clear();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the task has completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                lock (_taskLock)
                {
                    return _isCompleted;
                }
            }
        }

        /// <summary>
        /// Gets the result of the task. Blocks if the task has not yet completed.
        /// </summary>
        /// <exception cref="AggregateException">Thrown if the task completed with an exception.</exception>
        public TResult Result
        {
            get
            {
                lock (_taskLock)
                {
                    while (!_isCompleted)
                    {
                        Monitor.Wait(_taskLock); 
                    }
                    if (_exception != null)
                    {
                        throw new AggregateException(_exception);
                    }
                    return _result;
                }
            }
        }

        /// <summary>
        /// Creates a continuation task that will be started after this task completes.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
        /// <param name="continuation">The function to execute when this task completes.</param>
        /// <returns>A new task representing the continuation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="continuation"/> is null.</exception>
        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            ArgumentNullException.ThrowIfNull(continuation);

            var newTask = new MyTask<TNewResult>(() =>
            {
                try
                {
                    TResult res = this.Result;
                    return continuation(res);
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerException!;
                }
            });
            newTask.SetThreadPool(_threadPool);

            lock (_taskLock)
            {
                if (_isCompleted)
                {
                    _threadPool.EnqueueTask(() => newTask.Execute());
                }
                else
                {
                    _continuations.Add(() => newTask.Execute());
                }
            }

            return newTask;
        }
    }

    /// <summary>
    /// The worker method for threads in the pool. Continuously executes tasks from the queue.
    /// </summary>
    private void Work()
    {
        while (true)
        {
            Action task = null;
            lock (_lock)
            {
                while (_taskQueue.Count == 0 && !_isShutdown)
                {
                    Monitor.Wait(_lock);
                }

                if (_isShutdown && _taskQueue.Count == 0)
                {
                    return;
                }
                if (_taskQueue.Count > 0)
                {
                    task = _taskQueue.Dequeue();
                }
            }
            task?.Invoke();
        }
    }

    /// <summary>
    /// Submits a task to be executed by the thread pool.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="task">The function representing the task.</param>
    /// <returns>An <see cref="IMyTask{TResult}"/> representing the submitted task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the thread pool has been shut down.</exception>
    public IMyTask<TResult> SubmitTask<TResult>(Func<TResult> task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        lock (_lock)
        {
            if (_isShutdown)
            {
                throw new InvalidOperationException("ThreadPool has been shut down. Cannot accept new tasks.");
            }
        }

        var myTask = new MyTask<TResult>(task);
        myTask.SetThreadPool(this);

        lock (_lock)
        {
            _taskQueue.Enqueue(() => myTask.Execute());
            Monitor.Pulse(_lock); 
        }
        return myTask;
    }

    /// <summary>
    /// Shuts down the thread pool, waiting for all threads to complete.
    /// </summary>
    public void Shutdown()
    {
        lock (_lock)
        {
            _isShutdown = true;
            Monitor.PulseAll(_lock); 
        }

        foreach (var thread in _threads)
        {
            thread.Join(); 
        }
    }

    /// <summary>
    /// Enqueues a task to the task queue. Used internally for continuations.
    /// </summary>
    /// <param name="task">The task to enqueue.</param>
    /// <exception cref="InvalidOperationException">Thrown if the thread pool has been shut down.</exception>
    internal void EnqueueTask(Action task)
    {
        lock (_lock)
        {
            if (_isShutdown)
            {
                throw new InvalidOperationException("ThreadPool has been shut down. Cannot accept new tasks.");
            }
            _taskQueue.Enqueue(task);
            Monitor.Pulse(_lock); 
        }
    }
}


