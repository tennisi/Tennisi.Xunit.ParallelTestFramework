using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCollectionRunner"/>.
/// </summary>
/// <param name="testCollection">The test collection</param>
/// <param name="testCases">The test cases from the test collection</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="testCaseOrderer">The order used to order tests cases in the collection</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="assemblyFixtureMappings">The fixtures associated with the test assembly</param>
public class ParallelTestCollectionRunnerContext(
    IXunitTestCollection testCollection,
    IReadOnlyCollection<IXunitTestCase> testCases,
    ExplicitOption explicitOption,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    FixtureMappingManager assemblyFixtureMappings) :
    XunitTestCollectionRunnerBaseContext<IXunitTestCollection, IXunitTestCase>(testCollection, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings)
{ }
