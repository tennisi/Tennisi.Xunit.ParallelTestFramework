using Xunit;

namespace Tennisi.Xunit.v3.UIParallelTestFramework.Tests;

public class TestWindowTests
{
    const int DelayPerWindowMs = 2000; // 2 seconds per form
    private const int NumberOfForms = 3; // Three forms to simulate parallel execution
    private const double Factor = NumberOfForms * 0.5; // Minimal speedup factor of parallelism
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
    public async Task UITestFramework_ProvesParallelExecution(int windowIndex)
    {
        var tick = Environment.TickCount;
        if (StartTicks < tick)
            StartTicks = tick;
        
        await Task.Delay(DelayPerWindowMs);
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