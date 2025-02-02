using Xunit;

namespace Tennisi.v3.Tests;

[Collection("ParallelMultiClass")]
public class ParallelCollectionMultiClass1AttributeTests
{
    private readonly CollectionConcurrencyFixture fixture;

    public ParallelCollectionMultiClass1AttributeTests(CollectionConcurrencyFixture fixture)
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