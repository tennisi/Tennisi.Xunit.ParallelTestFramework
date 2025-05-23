﻿using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "General design")]
internal sealed class RetryTheoryDiscoverer : TheoryDiscoverer
{
    public RetryTheoryDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    { }

    protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[]? dataRow)
    {
        yield return 
            new RetryTestCase(DiagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(), 
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod,
                dataRow,theoryAttribute.GetRetryCount());
    }
}