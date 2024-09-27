using System.Collections.Concurrent;

namespace Tennisi.Xunit;

internal class ValueMonitor
{
    private TimeSpan _value;
    internal event Action<TimeSpan>? ValueChanged;
    private readonly object _lock = new();

    internal ValueMonitor()
    {
        _value = TimeSpan.Zero;
    }

    internal TimeSpan Value
    {
        get => _value;
        set
        {
            lock (_lock)
            {
                if (_value == value)
                    return;
                _value = value;
                ValueChanged?.Invoke(_value);
            }
        }
    }

    internal void ClearValueChangedHandler()
    {
        ValueChanged = null;
    }
}

internal static class ValueMonitorStorage
{
    private static readonly ConcurrentDictionary<ParallelTag, WeakReference<ValueMonitor>> MonitorStorage = new();
    
    public static ValueMonitor GetMonitor(ParallelTag parallelTag)
    {
        if (MonitorStorage.TryGetValue(parallelTag, out var weakReference) && 
            weakReference.TryGetTarget(out var monitor))
        {
            return monitor;
        }

        monitor = new ValueMonitor();
        weakReference = new WeakReference<ValueMonitor>(monitor);
        MonitorStorage.AddOrUpdate(parallelTag, weakReference, (key, oldValue) => weakReference);
        return monitor;
    }

    public static void Cleanup()
    {
        foreach (var key in MonitorStorage.Keys)
        {
            if (MonitorStorage.TryGetValue(key, out var weakReference) && !weakReference.TryGetTarget(out _))
            {
                MonitorStorage.TryRemove(key, out _);
            }
        }
    }
}

public static class ParallelTagExtensions
{
    /// <summary>
    /// Delays the simulation of time by a specified delay.
    /// This method increases the current time value in the associated <see cref="ParallelTag"/>
    /// by the given delay after awaiting a simulated delay.
    /// </summary>
    /// <param name="parallelTag">The <see cref="ParallelTag"/> associated with the monitor.</param>
    /// <param name="delay">The amount of time to delay.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="ParallelTag"/> has not been added to the storage.
    /// </exception>
    public static void SimulateDelay(this ParallelTag parallelTag, TimeSpan delay)
    {
        var monitor = ValueMonitorStorage.GetMonitor(parallelTag);
        monitor.Value += delay;
    }
    
    /// <summary>
    /// Gets the current simulated time for the associated <see cref="ParallelTag"/>.
    /// This represents the amount of time that has elapsed in the simulated environment.
    /// </summary>
    /// <param name="parallelTag">The <see cref="ParallelTag"/> associated with the monitor.</param>
    /// <returns>The current simulated time as a <see cref="TimeSpan"/>.</returns>
    public static TimeSpan SimulatedNow(this ParallelTag parallelTag)
    {
        var monitor = ValueMonitorStorage.GetMonitor(parallelTag);
        return monitor.Value;
    }
}