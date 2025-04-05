using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Unit.Tests;

[TestClass]
[SuppressMessage("Maintainability", "CA1515:Рассмотрите возможность сделать общедоступные типы внутренними")]
public class LimitedConcurrencyLevelTaskSchedulerTests
{
    [TestMethod]
    public void ConstructorCheckConstrains()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LimitedConcurrencyLevelTaskScheduler(0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LimitedConcurrencyLevelTaskScheduler(-1));
    }

    [TestMethod]
    public async Task ShouldSyncQueueTasksAndLimitConcurrency()
    {
        using var cts = new CancellationTokenSource();
        var observer = new Observer();
        observer.StartMonitoring(1, cts.Token);
        var concurrentTaskCount = 0;
        var peakConcurrency = 0;
        var completedTasks = 0;
        var maxConcurrency = 3;
        var scheduler = new   LimitedConcurrencyLevelTaskScheduler(maxConcurrency);
        var factory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.None,
            scheduler
        );
        
        var taskCount = 25;
        var tasks = Enumerable.Range(1, taskCount).Select(_ => 
                factory.StartNew(() =>
                {
                    var running = Interlocked.Increment(ref concurrentTaskCount);
                    Observer.Max(ref peakConcurrency, running);
                    Thread.Sleep(100);
                    Interlocked.Decrement(ref concurrentTaskCount);
                    Interlocked.Increment(ref completedTasks);
                }, CancellationToken.None, TaskCreationOptions.None, 
                    scheduler))
            .ToArray();
  
        await Task.WhenAll(tasks);
        
#if NET8_0
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif
        Assert.AreEqual(taskCount, completedTasks);
        Assert.IsTrue(peakConcurrency <= maxConcurrency, $"Peak concurrency {peakConcurrency} exceeded max {maxConcurrency}");
        Assert.IsTrue(observer.MaxObservedThreadCount <= maxConcurrency, $"Peak Independent concurrency {observer.MaxObservedThreadCount} exceeded max {maxConcurrency}");
    }
}