using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit.v2;

/// <inheritdoc />
internal class UiParallelTestAssemblyRunner : ParallelTestAssemblyRunner
{
    /// <inheritdoc />
    public UiParallelTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    {
    }

    /// <inheritdoc />
    protected override async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        var runner = new UiParallelTestTestCollectionRunner(
            testCollection,
            testCases,
            DiagnosticMessageSink,
            messageBus,
            TestCaseOrderer,
            new ExceptionAggregator(Aggregator),
            cancellationTokenSource);
        return await runner.RunAsync();
    }
}