using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/// <inheritdoc />
internal class ParallelTestFrameworkDiscoverer: XunitTestFrameworkDiscoverer
{
    /// <inheritdoc />
    public ParallelTestFrameworkDiscoverer(
        IXunitTestAssembly testAssembly,
        IXunitTestCollectionFactory? collectionFactory = null) :
        base(testAssembly, collectionFactory)
    {
    }
}