using Xunit;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Tests;

[CollectionDefinition("ParallelMultiClass")]
[TestParallelization]
public class ParallelMultiClassCollectionFixture : ICollectionFixture<CollectionConcurrencyFixture>
{
}