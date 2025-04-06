using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
internal class ParallelTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly AssemblyName _assemblyName;

    /// <inheritdoc />
    public ParallelTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider,
        IMessageSink diagnosticMessageSink)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
        _assemblyName = assemblyName;
    }

    /// <summary>
    /// Creates ParallelTestAssemblyRunner
    /// </summary>
    /// <param name="testCases"></param>
    /// <param name="executionMessageSink"></param>
    /// <param name="executionOptions"></param>
    /// <returns></returns>
    protected virtual XunitTestAssemblyRunner CreateRunner(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        return new ParallelTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
    }

    /// <inheritdoc />
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "By external requirement")]
    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        ParallelSettings.Instance.RefineParallelSetting(_assemblyName, executionOptions);
        using var assemblyRunner = CreateRunner(testCases, executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync();
    }
}