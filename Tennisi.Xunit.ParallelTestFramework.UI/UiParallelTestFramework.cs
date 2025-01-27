using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

/// <summary>
/// A custom implementation of the xUnit test framework tailored for GUI applications.
/// </summary>
/// <remarks>
/// The <c>ParallelTestFrameworkUI</c> is specifically designed to maximize parallel execution of 
/// <c>Fact</c> and <c>Theory</c> tests in applications with graphical user interfaces, such as WinForms and WPF.
/// It ensures that tests run efficiently while maintaining thread safety critical for GUI environments.
/// </remarks>
/// <example>
/// To use this framework in a GUI project, update your test project's assembly attribute:
/// <code>
/// [assembly: Xunit.TestFramework("Tennisi.Xunit.UiParallelTestFramework", "Tennisi.Xunit.ParallelTestFramework.UI")]
/// </code>
/// Combine with the <c>ParallelTag</c> instrument for optimized test tagging in parallelized scenarios.
/// </example>
internal class UiParallelTestFramework : ParallelTestFramework
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
