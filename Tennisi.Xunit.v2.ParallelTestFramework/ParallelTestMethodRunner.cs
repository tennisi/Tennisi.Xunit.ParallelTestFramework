using System.Collections.Concurrent;
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

    private TimeSpan _timeLimit = TimeSpan.FromSeconds(120);

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
            ParallelSettings.Instance.GetSetting(@Class.Assembly.Name, "xunit.execution.DisableParallelization");
    }

    /// <inheritdoc />
    protected override async Task<RunSummary?> RunTestCasesAsync()
    {
        bool disableParallelization;
        Limiter? limiter = null;
        if (_disableTestParallelizationOnAssembly)
        {
            disableParallelization = true;
        }
        else
        {
            disableParallelization =
                (
                    TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
                     && !TestMethod.TestClass.Class.IsEnabledParallelization())
                || TestMethod.TestClass.Class.IsDisabledParallelization() 
                || TestMethod.Method.IsDisabledParallelization()
                || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                    a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration))
                );
            limiter = ParallelSettings.Instance.GetLimiter(Class.Assembly.Name, TestMethod.TestClass);
        }
        
        var summary = new RunSummary();
        if (!disableParallelization && ParallelSettings.Instance.GetSetting(TestMethod.TestClass.Class.Assembly.Name, "xunit.discovery.PreEnumerateTheories"))
        {
            IEnumerable<RunSummary> caseSummaries;
            if (limiter != null)
            {
                var caseSummariesBag = new ConcurrentBag<RunSummary>();
                await Parallel.ForEachAsync(
                    TestCases,
                    async (testCase, ct) =>
                    {
                        await limiter.SemaphoreSlim.WaitAsync(ct);
                        try
                        {
                            var caseSummary =
                                await limiter.TaskFactory.StartNew(() => RunDiagnosticTestCaseAsync(testCase), CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach, scheduler: limiter.TaskScheduler).Unwrap();
                            caseSummariesBag.Add(caseSummary);
                        }
                        finally
                        {
                            limiter.SemaphoreSlim.Release();
                        }
                    });
                caseSummaries = caseSummariesBag;
            }
            else
            {
                var caseTasks = TestCases.Select(x => RunTestCaseAsync2(x, disableParallelization));
                var caseSummariesArr = await Task.WhenAll(caseTasks).ConfigureAwait(false);
                caseSummaries = caseSummariesArr;
            }
            
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
            return await RunDiagnosticTestCaseAsync(testCase);
        return await RunTestCaseAsync(testCase);
    }

    /// <inheritdoc />
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Xunit designed")]
    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var action = () => RunDiagnosticTestCaseAsync(testCase);
        if (SynchronizationContext.Current == null)
            return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
        var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        return await Task.Factory
                         .StartNew(action, CancellationTokenSource.Token,
                                   TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap()
                         .ConfigureAwait(false);
    }
    
    private async Task<RunSummary> RunDiagnosticTestCaseAsync(IXunitTestCase testCase)
    {
        var args = _constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();
        var parallelTag = ParallelTag.FromTestCase(_constructorArguments, testCase.UniqueID);
        if (parallelTag != null)
            ParallelTag.Inject(ref parallelTag, args);
        
        var parameters = testCase.TestMethodArguments != null
            ? string.Join(", ", testCase.TestMethodArguments.Select(a => a?.ToString() ?? "null"))
            : string.Empty;
        var testDetails = $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}({parameters})";

        try
        {
#if DEBUG
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"STARTED: {testDetails}"));
#endif
            using var timer = new Timer(_ => _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"WARNING: {testDetails} has been running for more than {Math.Round(_timeLimit.TotalMinutes, 2)} minutes")),
                null,
                _timeLimit,
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