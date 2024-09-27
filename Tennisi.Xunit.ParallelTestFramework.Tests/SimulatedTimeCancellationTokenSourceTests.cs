using Xunit;
using Xunit.Abstractions;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class SimulatedTimeCancellationTokenSourceTests
{
    private readonly ITestOutputHelper _testOutput;

    private readonly ParallelTag _tag;

    public SimulatedTimeCancellationTokenSourceTests(ITestOutputHelper testOutputHelper, ParallelTag parallelTag)
    {
        _testOutput = testOutputHelper;
        _tag = parallelTag;
    }
    
    [Fact]
    public void ItShouldIncreaseTime()
    {
        using var cts = new SimulatedTimeCancellationTokenSource(TimeSpan.Zero, _tag);
        var value = _tag.SimulatedNow();
        Assert.Equal(0, value.TotalSeconds);
        _tag.SimulateDelay(TimeSpan.FromSeconds(1));
        value = _tag.SimulatedNow();
        Assert.Equal(1, value.TotalSeconds);
    }
    
    [Fact]
    public void ItShouldRaise()
    {
        try
        {
            using var cts = new SimulatedTimeCancellationTokenSource(TimeSpan.Zero, _tag);
            cts.Cancel();
            var value = _tag.SimulatedNow();
            Assert.Equal(0, value.TotalSeconds);
            _tag.SimulateDelay(TimeSpan.FromSeconds(1));
            value = _tag.SimulatedNow();
            Assert.Equal(1, value.TotalSeconds);
            Assert.True(false);
        }
        catch (OperationCanceledException)
        {
            Assert.True(true);
        }
    }
}