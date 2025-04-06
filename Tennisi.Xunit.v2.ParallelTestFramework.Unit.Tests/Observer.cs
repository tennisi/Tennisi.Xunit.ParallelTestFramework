using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Unit.Tests;

internal sealed class Observer
{
    public ConcurrentBag<int> TestCases { get; } = new();
    
    public int RunningThreads;
    public int MaxObservedConcurrency;
    
    private int _independentMaxObservedThreads;
    private int _independentStartObservedThreads;
    public int IndependentMaxObservedThreadCount => - _independentStartObservedThreads + _independentMaxObservedThreads;

    private void TrackThreadUsage()
    {
        var currentThreads = Process.GetCurrentProcess().Threads.Count;
        _independentMaxObservedThreads = Math.Max(_independentMaxObservedThreads, currentThreads);
    }

    public void StartMonitoring(int delayMilliseconds, CancellationToken cancellationToken)
    {
        TrackThreadUsage();
        _independentStartObservedThreads = _independentMaxObservedThreads;
        Task.Run(() => MonitorThreadsForDuration(delayMilliseconds, cancellationToken), cancellationToken);
    }

    private void MonitorThreadsForDuration(int delayMilliseconds, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TrackThreadUsage();
            Thread.Sleep(delayMilliseconds);
        }
    }
    
    public static void Max(ref int target, int value)
    {
        int current;
        while ((current = Volatile.Read(ref target)) < value)
        {
            if (Interlocked.CompareExchange(ref target, value, current) == current)
                break;
        }
    }
}