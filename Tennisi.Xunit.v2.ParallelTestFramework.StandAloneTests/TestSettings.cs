namespace Tennisi.Xunit.v2.ParallelTestFramework.StandAloneTests;

internal sealed class TestSettings: ParallelSettings
{
    public int TestDegreeOfParallelism { get; set; }
    internal override Limiter ConstructLimiter(int degreeOfParallelism)
    {
        return base.ConstructLimiter(TestDegreeOfParallelism);
    }
}