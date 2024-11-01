using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

[Serializable]
internal class RetryTestCase : XunitTestCase
{
    private int _retryCount;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public RetryTestCase()
    {
    }

    public RetryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay,
        TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, int retryCount,
        object[] testMethodArguments)
        : base(diagnosticMessageSink, testMethodDisplay, defaultMethodDisplayOptions, testMethod,
            testMethodArguments: testMethodArguments)
    {
        _retryCount = retryCount;
    }
    
    public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var runCount = 0;

        while (true)
        {
            var delayedMessageBus = new DelayedMessageBus(messageBus);

            var summary = await base.RunAsync(diagnosticMessageSink, delayedMessageBus, constructorArguments,
                aggregator, cancellationTokenSource);
            if (aggregator.HasExceptions || summary.Failed == 0 || ++runCount >= _retryCount)
            {
                delayedMessageBus.Dispose();
                return summary;
            }

            diagnosticMessageSink.OnMessage(
                new DiagnosticMessage("Execution of '{0}' failed (attempt #{1}), test retrying...", DisplayName, runCount));
        }
    }

    public override void Serialize(IXunitSerializationInfo data)
    {
        base.Serialize(data);
        data.AddRetryCount(_retryCount);
    }

    public override void Deserialize(IXunitSerializationInfo data)
    {
        base.Deserialize(data);
        _retryCount = data.GetRetryCount();
    }
}