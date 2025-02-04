using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

internal static class ParallelExtensions
{
    private static bool IsDisabledParallelization(this Attribute attribute)
        => ((TestParallelizationAttribute)attribute).IsDisabledParallelization;
    private static bool IsEnabledParallelization(this Attribute attribute)
        => ((TestParallelizationAttribute)attribute).IsEnabledParallelization;
    
    private static bool IsDefaultParallelization(this Attribute attribute)
        => ((TestParallelizationAttribute)attribute).IsDefaultParallelization;

    private static bool IsDisabledParallelization(this IAttributeInfo? attributeInfo)
        => attributeInfo != null && ((ReflectionAttributeInfo)(attributeInfo)).Attribute.IsDisabledParallelization();
    private static bool IsEnabledParallelization(this IAttributeInfo? attributeInfo)
        => attributeInfo != null && ((ReflectionAttributeInfo)(attributeInfo)).Attribute.IsEnabledParallelization();
    private static bool IsDisabledParallelization(this IEnumerable<IAttributeInfo> attributes)
        => attributes.FirstOrDefault().IsDisabledParallelization();
    

    private static bool IsEnabledParallelization(this IEnumerable<IAttributeInfo> attributes)
        => attributes.FirstOrDefault().IsEnabledParallelization();

    internal static bool IsDisabledParallelization(this ITypeInfo typeInfo)
        => typeInfo.GetCustomAttributes(typeof(TestParallelizationAttribute)).IsDisabledParallelization();
    internal static bool IsEnabledParallelization(this ITypeInfo typeInfo)
        => typeInfo.GetCustomAttributes(typeof(TestParallelizationAttribute)).IsEnabledParallelization();

    internal static bool IsDisabledParallelization(this IMethodInfo typeInfo)
        => typeInfo.GetCustomAttributes(typeof(TestParallelizationAttribute)).IsDisabledParallelization();

    internal static bool IsEnabledParallelization(this IMethodInfo typeInfo)
        => typeInfo.GetCustomAttributes(typeof(TestParallelizationAttribute)).IsEnabledParallelization();
}