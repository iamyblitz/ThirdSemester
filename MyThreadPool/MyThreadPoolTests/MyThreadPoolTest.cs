namespace MyThreadPool;

public class MyThreadPoolTests
{
    [Test]
    public void SimpleTaskExecutionTest()
    {
        var pool = new MyThreadPool(2);

        var task = pool.SubmitTask(() => 100);

        Assert.That(task.Result, Is.EqualTo(100));

        pool.Shutdown();
    }
    
    [Test]
    public void TaskExceptionHandlingTest()
    {
        var pool = new MyThreadPool(2);

        var task = pool.SubmitTask<int>(() => throw new InvalidOperationException("Test exception"));

        Assert.Throws<AggregateException>(() =>
        {
            var result = task.Result;
        });

        pool.Shutdown();
    }
    
    [Test]
    public void ContinueWithTest()
    {
        var pool = new MyThreadPool(2);

        var task = pool.SubmitTask(() => 10);

        var continuation = task.ContinueWith(x => x * 2);

        Assert.That(continuation.Result, Is.EqualTo(20));

        pool.Shutdown();
    }
    
    [Test]
    public void ContinueWithAfterExceptionTest()
    {
        var pool = new MyThreadPool(2);

        var task = pool.SubmitTask<int>(() => throw new InvalidOperationException("Test exception"));

        var continuation = task.ContinueWith(x => x + 1);

        Assert.Throws<AggregateException>(() =>
        {
            var result = continuation.Result;
        });

        pool.Shutdown();
    }
    
    [Test]
    public void ShutdownTest()
    {
        var pool = new MyThreadPool(2);

        pool.Shutdown();

        Assert.Throws<InvalidOperationException>(() =>
        {
            pool.SubmitTask(() => 404);
        });
    }
    
    [Test]
    public void ConcurrentSubmitTest()
    {
        const int taskCount = 50;
        var pool = new MyThreadPool(4);
        IMyTask<int>[] tasks = new IMyTask<int>[taskCount];

        Parallel.For(0, taskCount, i =>
        {
            tasks[i] = pool.SubmitTask(() => i);
        });

        for (int i = 0; i < taskCount; i++)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(i));
        }

        pool.Shutdown();
    }
    
    [Test]
    public void ConcurrentResultAccessTest()
    {
        var pool = new MyThreadPool(2);
        var task = pool.SubmitTask(() =>
        {
            Thread.Sleep(200);
            return 123;
        });

        int concurrentAccessCount = 10;
        int[] results = new int[concurrentAccessCount];
        Thread[] threads = new Thread[concurrentAccessCount];

        for (int i = 0; i < concurrentAccessCount; i++)
        {
            int localIndex = i; 
            threads[i] = new Thread(() =>
            {
                results[localIndex] = task.Result;
            });
            threads[i].Start();
        }

        foreach (var t in threads)
        {
            t.Join();
        }

        for (int i = 0; i < concurrentAccessCount; i++)
        {
            Assert.That(results[i], Is.EqualTo(123));
        }

        pool.Shutdown();
    }
    
    [Test]
    public void ConcurrentSubmitTaskTest()
    {
        const int taskCount = 50;
        const int threadCount = 4;
        var pool = new MyThreadPool(threadCount);
        var tasks = new IMyTask<int>[taskCount];
        var threads = new Thread[taskCount];
        
        for (int i = 0; i < taskCount; i++)
        {
            int localIndex = i; 
            threads[i] = new Thread(() =>
            {
                tasks[localIndex] = pool.SubmitTask(() => localIndex);
            });
            threads[i].Start();
        }
        
        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        for (int i = 0; i < taskCount; i++)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(i), $"Task {i} returned incorrect result.");
        }

        pool.Shutdown();
    }
    
    [Test]
    public void ConcurrentContinueWithAndShutdownTest()
    {
        var pool = new MyThreadPool(4);
        var task = pool.SubmitTask(() =>
        {
            Thread.Sleep(100);
            return 42;
        });

        const int continuationCount = 20;
        IMyTask<int>[] continuations = new IMyTask<int>[continuationCount];
        var results = new int[continuationCount];
        var exceptions = new Exception[continuationCount];
        
        Parallel.For(0, continuationCount, i =>
        {
            continuations[i] = task.ContinueWith(x => x + i);
        });
        
        var resultThreads = new Thread[continuationCount];
        for (int i = 0; i < continuationCount; i++)
        {
            int localI = i;
            resultThreads[i] = new Thread(() =>
            {
                try
                {
                    results[localI] = continuations[localI].Result;
                }
                catch (Exception ex)
                {
                    exceptions[localI] = ex;
                }
            });
            resultThreads[i].Start();
        }
        
        Thread.Sleep(200);
        
        Thread shutdownThread = new Thread(() => pool.Shutdown());
        shutdownThread.Start();
        shutdownThread.Join();
        
        foreach (var thread in resultThreads)
        {
            thread.Join();
        }
        
        for (int i = 0; i < continuationCount; i++)
        {
            if (exceptions[i] == null)
            {
                Assert.That(results[i], Is.EqualTo(42 + i));
            }
            else
            {
                Assert.That(exceptions[i], Is.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf<OperationCanceledException>());
            }
        }
    }
}
