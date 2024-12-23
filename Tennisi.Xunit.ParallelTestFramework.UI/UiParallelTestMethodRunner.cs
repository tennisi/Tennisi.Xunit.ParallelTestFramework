using Xunit.Abstractions;
using Xunit.Sdk;
using System.Windows.Threading;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class UiParallelTestMethodRunner : ParallelTestMethodRunner
{
    /// <inheritdoc />
    public UiParallelTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object?[] constructorArguments)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator,
            cancellationTokenSource, constructorArguments)
    {
    }

    /// <inheritdoc />
    protected override async Task<RunSummary> RunExtendedTestCaseAsync(IXunitTestCase testCase, object?[] args)
    {
        var tcs = new TaskCompletionSource<RunSummary>();

        var thread = new Thread(() =>
        {
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;

                dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        var result = await base.RunExtendedTestCaseAsync(testCase, args);
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                    finally
                    {
                        Dispatcher.CurrentDispatcher.InvokeShutdown();
                    }
                });

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

#if WINDOWS
        thread.SetApartmentState(ApartmentState.STA);
#endif
        thread.IsBackground = true;
        thread.Start();
        thread.Join();
        return await tcs.Task;
    }
}
