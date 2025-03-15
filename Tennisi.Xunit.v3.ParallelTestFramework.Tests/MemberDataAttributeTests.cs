using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tennisi.v3.Tests;

public class MemberDataAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public static TheoryData<int> GetData() => new() { { 1 }, { 2 } };

    public MemberDataAttributeTests(ConcurrencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "By Author")]
    [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "By Author")]
    public async Task Theory(int value)
    {
        Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(true));
    }
}