using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class ParallelTestFrameworkExecutor(IXunitTestAssembly testAssembly) :
    TestFrameworkExecutor<IXunitTestCase>(testAssembly)
{
    readonly Lazy<ParallelTestFrameworkDiscoverer> discoverer = new(() => new(testAssembly));

    /// <summary>
    /// Gets the test assembly that contains the test.
    /// </summary>
    protected new IXunitTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

    /// <inheritdoc/>
    protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
        discoverer.Value;

    /// <inheritdoc/>
    public override async ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        await ParallelTestAssemblyRunner.Instance.Run(TestAssembly, testCases, executionMessageSink, executionOptions);
    }
}