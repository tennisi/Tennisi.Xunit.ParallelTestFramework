using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class ParallelTestFramework : XunitTestFramework
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "By Author")]
    protected ParallelTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        #if DEBUG
        messageSink?.OnMessage(new DiagnosticMessage("Using CustomTestFramework"));
        #endif
    }

    /// <inheritdoc />
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        return new ParallelTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }

    /// <inheritdoc />
    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
    {
        return new ParallelTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
    }
}
