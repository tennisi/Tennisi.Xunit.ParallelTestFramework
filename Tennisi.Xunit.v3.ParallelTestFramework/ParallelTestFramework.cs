using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/// <summary>
/// A custom implementation of the xUnit.v3 test framework designed to maximize parallelism.
/// </summary>
/// <remarks>
/// This framework enables full parallel execution of all <c>Fact</c> and <c>Theory</c> tests,
/// significantly improving execution speed compared to the default xUnit test framework.
/// </remarks>
/// <example>
/// To use this framework, update your test project's assembly attribute:
/// <code>
/// [assembly: Xunit.TestFramework("Tennisi.Xunit.v3.ParallelTestFramework", "Tennisi.Xunit.v3.ParallelTestFramework")]
/// </code>
/// Additionally, take advantage of the <c>ParallelTag</c> instrument to ensure unique tagging in parallelized tests.
/// </example>
public class ParallelTestFramework : XunitTestFramework
{
    private readonly string? _configFileName;
    
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "By Author")]
    public ParallelTestFramework(string? configFileName)
        : base(configFileName: configFileName)
    {
        _configFileName = configFileName;
    }
    
    /// <inheritdoc />
    public ParallelTestFramework()
    {
       
    }
    
    /// <inheritdoc />
    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
    {
        var result = new ParallelTestFrameworkDiscoverer(new XunitTestAssembly(Guard.ArgumentNotNull(assembly), _configFileName, assembly.GetName().Version));
        return result;
    }

    /// <inheritdoc />
    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        var result = new ParallelTestFrameworkExecutor(new XunitTestAssembly(Guard.ArgumentNotNull(assembly), _configFileName, assembly.GetName().Version));
        return result;
    }
}
