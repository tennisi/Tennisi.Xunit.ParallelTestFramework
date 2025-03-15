using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "General design")]
internal sealed class RetryFactDiscoverer : IXunitTestCaseDiscoverer
{
    public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        // yield return 
        //     new RetryTestCase(_diagnosticMessageSink,
        //         discoveryOptions.MethodDisplayOrDefault(), 
        //         discoveryOptions.MethodDisplayOptionsOrDefault(),
        //         testMethod,null, factAttribute.GetRetryCount());
       // throw new NotImplementedException();
       return new ValueTask<IReadOnlyCollection<IXunitTestCase>>();
    }
}