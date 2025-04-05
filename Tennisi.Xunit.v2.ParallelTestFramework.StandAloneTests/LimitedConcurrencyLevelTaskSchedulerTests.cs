using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tennisi.Xunit.v2.ParallelTestFramework.StandAloneTests;

internal class TestLimitedConcurrencyLevelTaskScheduler : LimitedConcurrencyLevelTaskScheduler
{
    public TestLimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        : base(maxDegreeOfParallelism) { }
        
    public new void QueueTask(Task task)
    {
        base.QueueTask(task);
    }

    public new bool TryDequeue(Task task)
    {
        return base.TryDequeue(task);
    }

    public new bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return base.TryExecuteTaskInline(task, taskWasPreviouslyQueued);
    }
        
    public new IEnumerable<Task> GetScheduledTasks()
    {
        return base.GetScheduledTasks();
    }
}

[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
[SuppressMessage("ReSharper", "ConvertConstructorToMemberInitializers")]
public class LimitedConcurrencyLevelTaskSchedulerTests
{
    private readonly TestLimitedConcurrencyLevelTaskScheduler _scheduler;

    public LimitedConcurrencyLevelTaskSchedulerTests()
    {
        _scheduler = new TestLimitedConcurrencyLevelTaskScheduler(2);
    }

    [TestMethod]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenMaxDegreeOfParallelismIsLessThanOne()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LimitedConcurrencyLevelTaskScheduler(0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LimitedConcurrencyLevelTaskScheduler(-1));
    }

    [TestMethod]
    public void Constructor_ShouldSetMaxDegreeOfParallelism_WhenValidValueIsProvided()
    {
        var scheduler = new LimitedConcurrencyLevelTaskScheduler(4);
        Assert.AreEqual(4, scheduler.MaximumConcurrencyLevel);
    }

    [TestMethod]
    [Ignore]
    public void QueueTask_ShouldQueueTasksAndLimitConcurrency()
    {
        var taskCount = 5;
        var completedTasks = 0;
        var concurrentTaskCount = 0;
        var maxConcurrency = _scheduler.MaximumConcurrencyLevel;
        var semaphore = new SemaphoreSlim(0, taskCount);

        var tasks = new List<Task>();

        for (var i = 0; i < taskCount; i++)
        {
            var task = new Task(() =>
            {
                Interlocked.Increment(ref concurrentTaskCount);
                Thread.Sleep(100);
                Interlocked.Decrement(ref concurrentTaskCount);

                Interlocked.Increment(ref completedTasks);
                semaphore.Release();
            });

            _scheduler.QueueTask(task);
            tasks.Add(task);
        }

        semaphore.Wait(taskCount);

        Assert.IsTrue(concurrentTaskCount <= maxConcurrency, "More than the maximum concurrency level was reached.");

        Assert.AreEqual(taskCount, completedTasks);

        semaphore.Dispose();
    }

    [TestMethod]
    public void TryExecuteTaskInline_ShouldReturnFalse_WhenThreadIsNotProcessingItems()
    {
        var task = new Task(() => { /* No operation */ });
        bool result = _scheduler.TryExecuteTaskInline(task, false);
        Assert.IsFalse(result);
    }

    [TestMethod]
    [Ignore]
    public void TryExecuteTaskInline_ShouldReturnTrue_WhenThreadIsProcessingItems()
    {
        var task1 = new Task(() => { });

        _scheduler.QueueTask(task1);

        task1.Start();

        bool result = _scheduler.TryExecuteTaskInline(task1, true);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryDequeue_ShouldReturnTrue_WhenTaskExistsInQueue()
    {
        var task = new Task(() => { /* No operation */ });
        _scheduler.QueueTask(task);

        bool result = _scheduler.TryDequeue(task);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryDequeue_ShouldReturnFalse_WhenTaskDoesNotExistInQueue()
    {
        var task = new Task(() => { /* No operation */ });

        bool result = _scheduler.TryDequeue(task);
        Assert.IsFalse(result);
    }

    [TestMethod]
    [Ignore]
    public void GetScheduledTasks_ShouldReturnTasksInQueue()
    {
        var task1 = new Task(() => { /* No operation */ });
        var task2 = new Task(() => { /* No operation */ });

        _scheduler.QueueTask(task1);
        _scheduler.QueueTask(task2);

        var scheduledTasks = _scheduler.GetScheduledTasks().ToList();
        Assert.AreEqual(2, scheduledTasks.Count);
        Assert.IsTrue(scheduledTasks.Contains(task1));
        Assert.IsTrue(scheduledTasks.Contains(task2));
    }

    [TestMethod]
    public void MaximumConcurrencyLevel_ShouldReturnMaxDegreeOfParallelism()
    {
        var scheduler = new LimitedConcurrencyLevelTaskScheduler(3);
        Assert.AreEqual(3, scheduler.MaximumConcurrencyLevel);
    }

    [TestMethod]
    public void TestProtectedMembers_AfterTaskQueued()
    {
        var task = new Task(() => { /* Simulate work */ });
        _scheduler.QueueTask(task);

        var tasks = _scheduler.Tasks;
        Assert.AreEqual(1, tasks.Count);
        Assert.IsTrue(tasks.Contains(task));

        var delegatesRunning = _scheduler.DelegatesQueuedOrRunning;
        Assert.AreEqual(1, delegatesRunning);
    }

    [TestMethod]
    [Ignore]
    public void TestMaxDegreeOfParallelism_LimitsConcurrency()
    {
        var taskCount = 5;
        var completedTasks = 0;

        var semaphore = new SemaphoreSlim(0, taskCount);

        for (var i = 0; i < taskCount; i++)
        {
            var task = new Task(() =>
            {
                Thread.Sleep(100);
                Interlocked.Increment(ref completedTasks);
                semaphore.Release();
            });
            _scheduler.QueueTask(task);
            task.Start();
        }

        semaphore.Wait(taskCount);

        Assert.AreEqual(taskCount, completedTasks);
        
        semaphore.Dispose();
    }
}