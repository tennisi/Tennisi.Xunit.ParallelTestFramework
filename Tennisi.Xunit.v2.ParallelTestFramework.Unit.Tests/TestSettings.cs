using Xunit.Abstractions;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Unit.Tests;

internal sealed class TestSettings: ParallelSettings
{
    private static readonly object LockObject = new();
    internal int TestDegreeOfParallelism { get; init; }
    private Limiter _limiter;
    internal override Limiter ConstructLimiter(int degreeOfParallelism)
    {
        lock (LockObject)
        {
            _limiter ??= base.ConstructLimiter(TestDegreeOfParallelism);
        }

        return _limiter;
    }
    
    internal override Limiter GetLimiter(string assemblyName, ITestClass testClass)
    {
        return ConstructLimiter(TestDegreeOfParallelism);
    }

    internal override bool GetSetting(string assemblyName, string setting)
    {
        return setting != "xunit.execution.DisableParallelization";
    }
}