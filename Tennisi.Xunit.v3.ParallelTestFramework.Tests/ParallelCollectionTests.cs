using Xunit;

namespace Tennisi.v3.Tests;

public class ParallelCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture fixture;

    public ParallelCollectionTests(ConcurrencyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void FactX()
    {
        Assert.True(true);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Fact2(int a)
    {
        Assert.True(true);
    }
}