using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
internal class UiParallelTestClassRunner : ParallelTestClassRunner 
{
    /// <inheritdoc />
    public UiParallelTestClassRunner(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings) : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
    {
    }

    /// <inheritdoc />
    protected override ParallelTestMethodRunner CreateMethodRunner(ITestMethod testMethod,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        object?[] constructorArguments)
    {
        return new UiParallelTestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink,
            MessageBus,
            new ExceptionAggregator(Aggregator),
            CancellationTokenSource, constructorArguments);
    }
}