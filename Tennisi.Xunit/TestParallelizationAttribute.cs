namespace Tennisi.Xunit;

/// <summary>
/// An attribute that controls test parallelization and the maximum number of test tasks running in parallel in the xUnit framework.
/// Can be applied at the assembly, test class, or test method level.
/// </summary>
/// <remarks>
/// <para>
/// When applied at the assembly level, setting the <see cref="Enabled"/> parameter to <c>true</c> enables test collection parallelization 
/// and theory pre-enumeration. Setting it to <c>false</c> disables theory pre-enumeration, test collection parallelization, 
/// and assembly parallelization while enabling global parallelization disabling. These behaviors can also be controlled through project file settings.
/// </para>
/// <para>
/// When applied at the test class or method level, it only controls whether that specific test class or method runs in parallel. 
/// By default, xUnit does not parallelize individual test classes, but enabling this setting allows parallel execution for the class.
/// </para>
/// <para>
/// The <see cref="DegreeOfParallelism"/> parameter controls the maximum number of test tasks running in parallel. 
/// If not specified, the number of tasks defaults to <see cref="Environment.ProcessorCount"/>.
/// If set to <c>-1</c>, the xUnit default settings are applied. Otherwise, the specified value is used as the limiter.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class TestParallelizationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class with a specified parallelization setting and degree.
    /// </summary>
    /// <param name="enabled">
    /// Specifies whether parallelization is enabled.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <param name="degreeOfParallelism">
    /// 1. If the degreeOfParallelism is not specified, the number of tasks is assumed to match Environment.ProcessorCount.
    /// 2. If the degreeOfParallelism is set to 0, the xUnit default settings are applied.
    /// 3. Otherwise, the specified value is used as the limiter.
    /// 
    /// </param>
    public TestParallelizationAttribute(bool enabled, int degreeOfParallelism)
    {
        Enabled = enabled;
        DegreeOfParallelism = degreeOfParallelism;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class with a string parallelization setting and degree.
    /// </summary>
    /// <param name="enabled">
    /// Specifies whether parallelization is enabled.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <param name="degreeOfParallelism">
    /// A string representation of the maximum number of test tasks to run in parallel.
    /// 1. If the degreeOfParallelism is not specified, the number of tasks is assumed to match Environment.ProcessorCount.
    /// 2. If the degreeOfParallelism is set to 0, the xUnit default settings are applied.
    /// 3. Otherwise, the specified value is used as the limiter.
    /// </param>
    public TestParallelizationAttribute(string enabled, string degreeOfParallelism)
    {
        Enabled = null;
        if (!string.IsNullOrEmpty(enabled) && bool.TryParse(enabled, out var result))
            Enabled = result;

        DegreeOfParallelism = null;
        if (!string.IsNullOrEmpty(degreeOfParallelism) && int.TryParse(degreeOfParallelism, out var resultDegree))
            DegreeOfParallelism = resultDegree;
    }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class with a string parallelization setting and degree.
    /// </summary>
    /// <param name="enabled">
    /// Specifies whether parallelization is enabled.
    /// Defaults to <c>true</c>.
    /// </param>
    public TestParallelizationAttribute(string enabled)
    {
        Enabled = null;
        if (!string.IsNullOrEmpty(enabled) && bool.TryParse(enabled, out var result))
            Enabled = result;
        DegreeOfParallelism = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class with a string parallelization setting and degree.
    /// </summary>
    /// <param name="enabled">
    /// Specifies whether parallelization is enabled.
    /// Defaults to <c>true</c>.
    /// </param>
    public TestParallelizationAttribute(bool enabled)
    {
        Enabled = enabled;
        DegreeOfParallelism = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class.
    /// </summary>
    public TestParallelizationAttribute()
    {
        Enabled = null;
        DegreeOfParallelism = null;
    }

    /// <summary>
    /// Gets the parallelization setting.
    /// </summary>
    public bool? Enabled { get; }

    /// <summary>
    /// 1. If the degreeOfParallelism is not specified, the number of tasks is assumed to match Environment.ProcessorCount.
    /// 2. If the degreeOfParallelism is set to 0, the xUnit default settings are applied.
    /// 3. Otherwise, the specified value is used as the limiter.
    /// </summary>
    public int? DegreeOfParallelism { get; }
    
    internal bool IsDefaultParallelization => Enabled == true;
    internal bool IsEnabledParallelization => Enabled is null;
    internal bool IsDisabledParallelization => Enabled == false;
    internal bool IsFullParallelization => Enabled == true;
}