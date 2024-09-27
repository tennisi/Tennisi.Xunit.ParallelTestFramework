namespace Tennisi.Xunit;

/// <summary>
/// A custom implementation of <see cref="CancellationTokenSource"/> that works with simulated or fake time.
/// This class provides cancellation capabilities based on a manipulated time source.
/// </summary>
public class SimulatedTimeCancellationTokenSource : CancellationTokenSource
{
    private readonly ValueMonitor _monitor;
    private TimeSpan _cancelDelay;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedTimeCancellationTokenSource"/> class with a specified time limit
    /// and an optional time-sharer object to coordinate the simulated time across multiple components.
    /// </summary>
    /// <param name="delay">
    /// The amount of time to wait before the cancellation is automatically triggered. Functions similarly to the 
    /// <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> method.
    /// </param>
    /// <param name="parallelTag">
    /// <see cref="ParallelTag"/> object used to share and coordinate the simulated time with other components. 
    /// It ensures that related components progress in simulated time in a synchronized manner.
    /// </param>
    public SimulatedTimeCancellationTokenSource(TimeSpan delay, ParallelTag parallelTag)
    {
        _monitor = ValueMonitorStorage.GetMonitor(parallelTag);
        _cancelDelay = delay;
        SetupValueChangedHandler();
    }

    /// <summary>
    /// Cancels the simulated cancellation token after the specified delay.
    /// </summary>
    /// <param name="delay">The <see cref="TimeSpan"/> interval after which the cancellation should occur.</param>
    /// <remarks>
    /// This method overrides the base <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> method
    /// but does not call the base implementation. Instead, it provides custom behavior tailored for
    /// the simulated environment, managing cancellation timing according to the simulated context.
    /// </remarks>
    public new void CancelAfter(TimeSpan delay)
    {
        _cancelDelay = delay;
        SetupValueChangedHandler(); 
    }

    private void SetupValueChangedHandler()
    {
        _monitor.ClearValueChangedHandler(); 
        _monitor.ValueChanged += _ =>
        {
            if (_monitor.Value >= _cancelDelay)
            {
                Token.ThrowIfCancellationRequested();
                Cancel();
            }
        };
    }
}