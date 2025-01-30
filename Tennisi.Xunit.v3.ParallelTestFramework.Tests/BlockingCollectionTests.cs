using Xunit;

namespace Tennisi.v3.Tests;

public class BlockingCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public BlockingCollectionTests(ConcurrencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Fact1()
    {
        Assert.Equal(2, await _fixture.CheckConcurrencyAsync());
    }

    [Fact]
    public async Task Fact2()
    {
        Assert.Equal(2,await _fixture.CheckConcurrencyAsync());
    }
}