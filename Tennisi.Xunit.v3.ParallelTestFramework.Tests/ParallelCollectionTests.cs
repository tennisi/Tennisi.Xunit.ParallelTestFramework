using Xunit;

namespace Tennisi.v3.Tests;

public class ParallelCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public ParallelCollectionTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact(Skip = "Not implemented yet")]
    public async Task Fact1()
    {
        Assert.Equal(2, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }

    [Fact(Skip = "Not implemented yet")]
    public async Task Fact2()
    {
        Assert.Equal(2, await fixture.CheckConcurrencyAsync().ConfigureAwait(false));
    }
}