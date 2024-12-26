using System.Windows;
using Xunit;
using Xunit.Abstractions;

namespace Tennisi.Xunit.UIParallelTestFramework.Tests;

public class TestWindowTests
{
    const int DelayPerWindowMs = 2000; // 2 seconds per form
    private const int NumberOfForms = 10; // Three forms to simulate parallel execution
    private const double Factor = NumberOfForms * 0.75; // Minimal speedup factor of parallelism
    private const int SerialTime = DelayPerWindowMs * NumberOfForms;

    private static volatile int StartTicks = int.MinValue;
    private static volatile int Ready = NumberOfForms;

    private readonly ITestOutputHelper _testOutputHelper;
    public TestWindowTests(ITestOutputHelper output)
    {
        _testOutputHelper = output;
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public async Task UITestFramework_ProvesParallelExecution(int windowIndex)
    {
        var tick = Environment.TickCount;
        if (StartTicks < tick)
            StartTicks = tick;

        var window = new Window
        {
            Title = $"Window {windowIndex + 1}",
            Width = 300,
            Height = 200
        };
        window.Show();
        await Task.Delay(DelayPerWindowMs);
        window.Title = $"Window {windowIndex + 1}";
        Assert.Equal($"Window {windowIndex + 1}", window.Title);
        window.Close();

        Interlocked.Decrement(ref Ready);

        if (Ready == 0)
        {
            var totalTime = Environment.TickCount - tick;
            var msg = $"Expected parallel execution to be faster than serial at least in {Factor} times. " +
                      $"Actual: {totalTime}ms, Expected: < {SerialTime}ms.";
            Assert.True(totalTime * Factor < SerialTime, msg);
            _testOutputHelper.WriteLine(msg);
        }
    }
}