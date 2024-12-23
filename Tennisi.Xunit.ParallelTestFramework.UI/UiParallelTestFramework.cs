using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class UiParallelTestFramework : ParallelTestFramework
{
    /// <inheritdoc />
    public UiParallelTestFramework(IMessageSink messageSink) : base(messageSink)
    {
    }

    /// <inheritdoc />
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        return new UiParallelTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }

    /// <inheritdoc />
    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
    {
        return new UiParallelTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
    }
}
