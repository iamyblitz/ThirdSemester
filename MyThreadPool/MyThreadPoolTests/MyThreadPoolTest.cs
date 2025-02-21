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
    public void ThreadCountTest()
    {
        int threadCount = 3;
        var pool = new MyThreadPool(threadCount);

        int runningThreads = 0;
        var resetEvent = new ManualResetEvent(false);

        for (int i = 0; i < threadCount; i++)
        {
            pool.SubmitTask(() =>
            {
                Interlocked.Increment(ref runningThreads);
                resetEvent.WaitOne(); 
                Interlocked.Decrement(ref runningThreads);
                return true;
            });
        }
        
        Thread.Sleep(500);

        Assert.That(runningThreads, Is.EqualTo(threadCount));
        resetEvent.Set();
        pool.Shutdown();
    }
}