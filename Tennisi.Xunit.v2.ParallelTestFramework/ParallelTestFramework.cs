using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <summary>
/// A custom implementation of the xUnit test framework designed to maximize parallelism.
/// </summary>
/// <remarks>
/// This framework enables full parallel execution of all <c>Fact</c> and <c>Theory</c> tests,
/// significantly improving execution speed compared to the default xUnit test framework.
/// </remarks>
/// <example>
/// To use this framework, update your test project's assembly attribute:
/// <code>
/// [assembly: Xunit.TestFramework("Tennisi.Xunit.v2.ParallelTestFramework", "Tennisi.Xunit.v2.ParallelTestFramework")]
/// </code>
/// Additionally, take advantage of the <c>ParallelTag</c> instrument to ensure unique tagging in parallelized tests.
/// </example>
internal class ParallelTestFramework : XunitTestFramework
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
