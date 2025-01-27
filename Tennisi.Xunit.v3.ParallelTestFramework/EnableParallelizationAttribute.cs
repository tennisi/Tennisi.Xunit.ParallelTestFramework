namespace Tennisi.Xunit.v3;

/// <summary>
/// Attribute that enables parallel test execution for a class, 
/// overriding any higher-level settings that disable parallelization.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EnableParallelizationAttribute : Attribute
{
}
