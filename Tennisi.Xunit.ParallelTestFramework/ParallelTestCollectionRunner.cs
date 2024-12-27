using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
internal class ParallelTestTestCollectionRunner : XunitTestCollectionRunner
{
    /// <inheritdoc />
    public ParallelTestTestCollectionRunner(ITestCollection testCollection,
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
    
    /// <summary>
    /// Creates a custom test class runner for executing tests in parallel within a class.
    /// </summary>
    /// <param name="testClass">The test class to be executed.</param>
    /// <param name="classT">Reflection information about the test class.</param>
    /// <param name="testCases">The collection of test cases to execute within the class.</param>
    /// <returns>
    /// A <see cref="ParallelTestClassRunner"/> instance responsible for executing tests in the given test class.
    /// </returns>
    /// <remarks>
    /// This method can be overridden to provide a custom implementation of the test class runner,
    /// enabling specialized behavior for running tests within a class.
    /// </remarks>
    protected virtual ParallelTestClassRunner CreateClassRunner(ITestClass testClass, IReflectionTypeInfo classT,
        IEnumerable<IXunitTestCase> testCases)
    {
        return new ParallelTestClassRunner(testClass, classT, testCases, DiagnosticMessageSink, MessageBus,
            TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource,
            CollectionFixtureMappings);
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class,
        IEnumerable<IXunitTestCase> testCases)
        => CreateClassRunner(testClass, @class, testCases)
            .RunAsync();

    /// <inheritdoc />
    protected override async Task<RunSummary> RunTestClassesAsync()
    {
        if (TestCollection.CollectionDefinition == null)
        {
            var summary = new RunSummary();

            var classTasks = TestCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance)
                .Select(tc => RunTestClassAsync(tc.Key, (IReflectionTypeInfo)tc.Key.Class, tc));

            var classSummaries = await Task.WhenAll(classTasks)
#if !NETSTANDARD
                    .WaitAsync(CancellationTokenSource.Token)
#endif
                .ConfigureAwait(false);
            foreach (var classSummary in classSummaries)
            {
                summary.Aggregate(classSummary);
            }

            return summary;
        }

        // Fall back to default behavior
        return await base.RunTestClassesAsync().ConfigureAwait(false);
    }
}
