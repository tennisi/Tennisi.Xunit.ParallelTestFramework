using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class UiParallelTestTestCollectionRunner : ParallelTestTestCollectionRunner
{
    /// <inheritdoc />
    public UiParallelTestTestCollectionRunner(ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator,
            cancellationTokenSource)
    {
    }

    /// <inheritdoc />
    protected override ParallelTestClassRunner CreateClassRunner(ITestClass testClass, IReflectionTypeInfo classT,
        IEnumerable<IXunitTestCase> testCases)
    {
        return new UiParallelTestClassRunner(testClass, classT, testCases, DiagnosticMessageSink, MessageBus,
            TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource,
            CollectionFixtureMappings);
    }
}
