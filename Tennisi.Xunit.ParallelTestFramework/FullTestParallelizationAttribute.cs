namespace Tennisi.Xunit;

/// <summary>
/// Форсирует параметры xUnit <c>parallelizeTestCollections</c> и <c>preEnumerateTheories</c> в положение 'включен'.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class FullTestParallelizationAttribute : Attribute
{
}