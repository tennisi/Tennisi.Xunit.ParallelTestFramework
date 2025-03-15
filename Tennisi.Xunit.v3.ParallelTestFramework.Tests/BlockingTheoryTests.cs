using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tennisi.v3.Tests;

public class BlockingTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public BlockingTheoryTests(ConcurrencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory(Skip = "Not implemented yet")]
    [InlineData(1)]
    [InlineData(2)]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "By Author")]
    [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "By Author")]
    public async Task Theory(int value)
    {
        Assert.Equal(2, await _fixture.CheckConcurrencyAsync());
    }
}