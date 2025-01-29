using Xunit;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Tests;

[CollectionDefinition("ParallelMultiClass")]
[EnableParallelization]
public class ParallelMultiClassCollectionFixture : ICollectionFixture<CollectionConcurrencyFixture>
{
}