using Tennisi.Xunit;
using Xunit;

namespace Tennisi.v3.Tests;

[Collection("Parallel")]
[EnableParallelization]
public class ParallelCollectionAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public ParallelCollectionAttributeTests(ConcurrencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Not implemented yet")]
    public async Task Fact1()
    {
        Assert.Equal(2, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }

    [Fact(Skip = "Not implemented yet")]
    public async Task Fact2()
    {
        Assert.Equal(2, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }
}