using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class ParallelTestFrameworkDiscoverer: XunitTestFrameworkDiscoverer
{
    private readonly IXunitTestAssembly _testAssembly;
    /// <inheritdoc />
    public ParallelTestFrameworkDiscoverer(
        IXunitTestAssembly testAssembly,
        IXunitTestCollectionFactory? collectionFactory = null) :
        base(testAssembly, collectionFactory)
    {
        _testAssembly = testAssembly;
    }

    /// <inheritdoc />
    protected override ValueTask<bool> FindTestsForType(IXunitTestClass testClass, ITestFrameworkDiscoveryOptions discoveryOptions,
        Func<ITestCase, ValueTask<bool>> discoveryCallback)
    {
        ParallelSettings.RefineParallelSetting(_testAssembly.AssemblyName, discoveryOptions);
        return base.FindTestsForType(testClass, discoveryOptions, discoveryCallback);
    }
}