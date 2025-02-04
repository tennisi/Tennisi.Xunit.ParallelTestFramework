namespace Tennisi.Xunit;

/// <summary>
/// An attribute that controls test parallelization in the xUnit framework.
/// Can be applied at the assembly or test class level.
/// </summary>
/// <remarks>
/// <para>
/// When applied at the assembly level, setting the parameter to <c>true</c> 
/// enables test collection parallelization and theory pre-enumeration. 
/// Setting it to <c>false</c> disables theory pre-enumeration, test collection parallelization, 
/// and assembly parallelization while enabling global parallelization disabling.
/// These behaviors can also be controlled through project file settings.
/// </para>
/// <para>
/// When applied at the test class or method level, it only controls whether that specific 
/// test class or method runs in parallel. By default, xUnit does not parallelize individual test classes, 
/// but enabling this setting allows parallel execution for the class.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class TestParallelizationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class.
    /// </summary>
    /// <param name="enabled">
    /// Specifies whether parallelization is enabled.
    /// Defaults to <c>true</c>.
    /// </param>
    public TestParallelizationAttribute(bool enabled)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class.
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
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestParallelizationAttribute"/> class.
    /// </summary>
    public TestParallelizationAttribute()
    {
        Enabled = null;
    }

    /// <summary>
    /// An attribute that controls test parallelization in the xUnit framework.
    /// Can be applied at the assembly or test class level.
    /// </summary>
    public bool? Enabled { get; }

    internal bool IsDefaultParallelization => Enabled == true;
    internal bool IsEnabledParallelization => Enabled is null;
    internal bool IsDisabledParallelization => Enabled == false;
    internal bool IsFullParallelization => Enabled == true;
}
