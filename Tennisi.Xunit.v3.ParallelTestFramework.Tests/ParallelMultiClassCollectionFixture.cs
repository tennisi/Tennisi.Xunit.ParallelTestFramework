using Tennisi.Xunit;
using Xunit;

namespace Tennisi.v3.Tests;

[CollectionDefinition("ParallelMultiClass")]
[EnableParallelization]
public class ParallelMultiClassCollectionFixture : ICollectionFixture<CollectionConcurrencyFixture>
{
}