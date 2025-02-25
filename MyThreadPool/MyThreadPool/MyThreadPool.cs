// <copyright file="MyThreadPool.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;

namespace MyThreadPool;

/// <summary>
/// Represents a thread pool that manages a fixed number of threads to execute submitted tasks.
/// </summary>
public class MyThreadPool
{
    private readonly Thread[] _threads;
    private readonly Queue<Action> _taskQueue = new();
    private readonly object _lock = new object();
    private bool _isShutdown = false;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class and starts the specified number of threads.
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
    /// Enqueues a continuation task without checking if the thread pool has been shut down.
    /// This ensures that all continuations are executed even if the pool is in shutdown mode.
    /// </summary>
    /// <param name="task">The continuation task to enqueue.</param>
    private void EnqueueContinuationTask(Action task)
    {
        lock (_lock)
        {
            _taskQueue.Enqueue(task);
            Monitor.Pulse(_lock);
        }
    }

    /// <summary>
    /// Enqueues a new task to be executed by the thread pool.
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

    /// <summary>
    /// The worker method for the threads in the pool.
    /// Continuously executes tasks from the queue until the pool is shut down and the queue is empty.
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
    /// The task is represented as a function returning a result of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result produced by the task.</typeparam>
    /// <param name="task">The function representing the task to execute.</param>
    /// <returns>An <see cref="IMyTask{TResult}"/> representing the submitted task.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided task is null.</exception>
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
            var myTask = new MyTask<TResult>(task, this, _cancellationTokenSource.Token); 
            _taskQueue.Enqueue(() => myTask.Execute());
            Monitor.Pulse(_lock);
            return myTask;
        }
    }

    /// <summary>
    /// Shuts down the thread pool and waits for all threads to complete.
    /// No new tasks are accepted once shutdown is initiated.
    /// </summary>
    public void Shutdown()
    {
        lock (_lock)
        {
            _isShutdown = true;
            _cancellationTokenSource.Cancel();
            Monitor.PulseAll(_lock);
        }

        foreach (var thread in _threads)
        {
            thread.Join();
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
        private Func<TResult>? _task; 
        private readonly object _taskLock = new object();
        private readonly List<Action> _continuations = new List<Action>();
        private Exception _exception;
        private readonly MyThreadPool _threadPool;
        private readonly CancellationToken _cancellationToken;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class with the specified task function and thread pool.
        /// </summary>
        /// <param name="task">The function representing the task to be executed.</param>
        /// <param name="threadPool">The thread pool that will execute the task.</param>
        /// <exception cref="ArgumentNullException">Thrown if either the task or threadPool is null.</exception>
        public MyTask(Func<TResult> task, MyThreadPool threadPool, CancellationToken cancellationToken) 
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _threadPool = threadPool ?? throw new ArgumentNullException(nameof(threadPool));
            _cancellationToken = cancellationToken;
            _isCompleted = false;
        }

        /// <summary>
        /// Executes the task and processes any registered continuations.
        /// After the task is executed, the task function reference is cleared to free up memory.
        /// </summary>
        public void Execute()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                lock (_taskLock)
                {
                    _exception = new OperationCanceledException("Task was canceled due to thread pool shutdown.");
                    _isCompleted = true;
                    Monitor.PulseAll(_taskLock);
                    _task = null;
                }
                return;
            }
            try
            {
                TResult res = _task!.Invoke();
                lock (_taskLock)
                {
                    _result = res;
                    _isCompleted = true;
                    Monitor.PulseAll(_taskLock);
                    _task = null; 
                }
            }
            catch (Exception ex)
            {
                lock (_taskLock)
                {
                    _exception = ex;
                    _isCompleted = true;
                    Monitor.PulseAll(_taskLock);
                    _task = null; 
                }
            }
            ExecuteContinuations();
        }

        /// <summary>
        /// Executes any continuation tasks that were registered to run after this task completes.
        /// Continuations are enqueued using a method that does not check for shutdown, ensuring they always run.
        /// </summary>
        private void ExecuteContinuations()
        {
            lock (_taskLock)
            {
                foreach (var continuation in _continuations)
                {
                    _threadPool.EnqueueContinuationTask(continuation); 
                }
                _continuations.Clear();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this task has completed execution.
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
        /// Gets the result of the task, blocking the calling thread until the task completes.
        /// If the task completed with an exception, accessing this property throws an AggregateException.
        /// </summary>
        /// <exception cref="AggregateException">Thrown if the task encountered an exception during execution.</exception>
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
        /// Creates a continuation task that will execute after the current task completes.
        /// The continuation task will receive the result of this task as its argument.
        /// </summary>
        /// <typeparam name="TNewResult">The type of result produced by the continuation task.</typeparam>
        /// <param name="continuation">A function to execute as the continuation.</param>
        /// <returns>An <see cref="IMyTask{TNewResult}"/> representing the continuation task.</returns>
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
            }, _threadPool, _cancellationToken);

            lock (_taskLock)
            {
                if (_isCompleted)
                {
                    _threadPool.EnqueueContinuationTask(() => newTask.Execute());
                }
                else
                {
                    _continuations.Add(() => newTask.Execute());
                }
            }
            return newTask;
        }
    }
}

