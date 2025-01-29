using Xunit;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Tests;

[DisableParallelization]
public class SequentialCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public SequentialCollectionTests(ConcurrencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Fact1()
    {
        Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }

    [Fact]
    public async Task Fact2()
    {
        Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }
}