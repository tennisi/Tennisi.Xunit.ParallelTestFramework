namespace Tennisi.Xunit.v2;

/// <summary>
/// Attribute that disables parallel test execution for a class, 
/// overriding any higher-level settings that enable parallelization.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DisableParallelizationAttribute : Attribute
{
}
