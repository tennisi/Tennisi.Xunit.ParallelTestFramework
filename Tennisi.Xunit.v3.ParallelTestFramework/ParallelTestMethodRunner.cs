﻿using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/// <summary>
/// The test method runner for xUnit.net v3 tests.
/// </summary>
public class ParallelTestMethodRunner : XunitTestMethodRunnerBase<ParallelTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XunitTestMethodRunner"/> class.
    /// </summary>
    protected ParallelTestMethodRunner()
    { }

    /// <summary>
    /// Gets the singleton instance of the <see cref="XunitTestMethodRunner"/> class.
    /// </summary>
    public static ParallelTestMethodRunner Instance { get; } = new();

    /// <summary>
    /// Runs the test test method.
    /// </summary>
    /// <param name="testMethod">The test method to be run.</param>
    /// <param name="testCases">The test cases to be run. Cannot be empty.</param>
    /// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
    /// <param name="messageBus">The message bus to report run status to.</param>
    /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
    /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
    /// <param name="constructorArguments">The constructor arguments for the test class.</param>
    public async ValueTask<RunSummary> Run(
        IXunitTestMethod testMethod,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object?[] constructorArguments)
    {
        Guard.ArgumentNotNull(testCases);
        Guard.ArgumentNotNull(messageBus);
        Guard.ArgumentNotNull(constructorArguments);

        await using var ctxt = new ParallelTestMethodRunnerContext(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, constructorArguments);
        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }
}
