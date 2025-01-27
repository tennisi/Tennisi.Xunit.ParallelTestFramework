using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/*
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "General design")]
internal sealed class RetryFactDiscoverer : IXunitTestCaseDiscoverer
{
    readonly IMessageSink _diagnosticMessageSink;

    public RetryFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        yield return 
            new RetryTestCase(_diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(), 
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod,null, factAttribute.GetRetryCount());
    }
}*/