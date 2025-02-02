using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "General design")]
internal sealed class RetryTheoryDiscoverer : TheoryDiscoverer
{
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        // yield return 
        //     new RetryTestCase(DiagnosticMessageSink,
        //         discoveryOptions.MethodDisplayOrDefault(), 
        //         discoveryOptions.MethodDisplayOptionsOrDefault(),
        //         testMethod,
        //         dataRow,theoryAttribute.GetRetryCount());
        return base.CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments);
    }

    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute)
    {
        return base.CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
    }
}