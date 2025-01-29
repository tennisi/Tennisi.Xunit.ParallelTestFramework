using Xunit;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Tests;

[Collection("Parallel")]
[EnableParallelization]
public class ParallelCollectionAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public ParallelCollectionAttributeTests(ConcurrencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Fact1()
    {
        Assert.Equal(2, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }

    [Fact]
    public async Task Fact2()
    {
        Assert.Equal(2, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }
}