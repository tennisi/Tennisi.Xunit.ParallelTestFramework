using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
internal class ParallelTestMethodRunner : XunitTestMethodRunner
{
    private readonly bool _disableTestParallelizationOnAssembly;
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly object?[] _constructorArguments;

    private TimeSpan TimeLimit = TimeSpan.FromSeconds(120);

    /// <inheritdoc />
    public ParallelTestMethodRunner(ITestMethod testMethod,
        IReflectionTypeInfo @class,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object?[] constructorArguments)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator,
            cancellationTokenSource, constructorArguments)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
        _constructorArguments = constructorArguments;
        _disableTestParallelizationOnAssembly = 
            ParallelSettings.GetSetting(@Class.Assembly.Name, "xunit.execution.DisableParallelization");

    }

    /// <inheritdoc />
    protected override async Task<RunSummary?> RunTestCasesAsync()
    {
        var disableParallelization =
            _disableTestParallelizationOnAssembly ||
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
            || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration)));
        
        var summary = new RunSummary();
        if (!disableParallelization && ParallelSettings.GetSetting(TestMethod.TestClass.Class.Assembly.Name, "xunit.discovery.PreEnumerateTheories"))
        {
            var caseTasks = TestCases.Select(x => RunTestCaseAsync2(x, disableParallelization));
            var caseSummaries = await Task.WhenAll(caseTasks).ConfigureAwait(false);
            foreach (var caseSummary in caseSummaries)
            {
                summary.Aggregate(caseSummary);
            }
        }
        else
        {
            foreach (var xunitTestCase in TestCases)
            {
                summary.Aggregate(await RunTestCaseAsync2(xunitTestCase, disableParallelization));
                if (CancellationTokenSource.IsCancellationRequested)
                    break;
            }
            return summary;
        }

        return summary;
    }
    
    private async Task<RunSummary> RunTestCaseAsync2(IXunitTestCase testCase, bool disableParallelization)
    {
        if (disableParallelization)
            return await RunDiagnosticTestCaseAsync(testCase, _constructorArguments);
        return await RunTestCaseAsync(testCase);
    }

    /// <inheritdoc />
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Xunit designed")]
    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var args = _constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();
        var parallelTag = ParallelTag.FromTestCase(_constructorArguments, testCase);
        if (parallelTag != null)
            ParallelTag.Inject(ref parallelTag, ref args);

        var action = () => RunDiagnosticTestCaseAsync(testCase, args);
        if (SynchronizationContext.Current == null)
            return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
        var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        return await Task.Factory
                         .StartNew(action, CancellationTokenSource.Token,
                                   TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap()
                         .ConfigureAwait(false);
    }
    
    private async Task<RunSummary> RunDiagnosticTestCaseAsync(IXunitTestCase testCase, object?[] args)
    {
        var parameters = testCase.TestMethodArguments != null
            ? string.Join(", ", testCase.TestMethodArguments.Select(a => a?.ToString() ?? "null"))
            : string.Empty;
        var testDetails = $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}({parameters})";

        try
        {
#if DEBUG
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"STARTED: {testDetails}"));
#endif
            using var timer = new Timer(_ => _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"WARNING: {testDetails} has been running for more than {Math.Round(TimeLimit.TotalMinutes, 2)} minutes")),
                null,
                TimeLimit,
                Timeout.InfiniteTimeSpan);

            var result = await RunExtendedTestCaseAsync(testCase, args);
#if DEBUG
            var status = result.Failed > 0 ? "FAILURE" : result.Skipped > 0 ? "SKIPPED" : "SUCCESS";
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"{status}: {testDetails} ({result.Time}s)"));
#endif
            return result;
        }
        catch (Exception ex)
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"ERROR: {testDetails} ({ex.Message})"));
            throw;
        }
    }

    /// <summary>
    /// RunExtendedTestCaseAsync
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    protected virtual async Task<RunSummary> RunExtendedTestCaseAsync(IXunitTestCase testCase, object?[] args)
    {
        return await testCase.RunAsync(_diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator),
            CancellationTokenSource);
    }
}