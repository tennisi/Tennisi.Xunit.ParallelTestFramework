using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class RetryCache
{
    private static readonly ConcurrentDictionary<(ITestMethod? Method, ITypeInfo? Class), RetryEntry> Cache = new();

    internal static bool ShouldUseClassRetry(this ITestMethod testMethod, out int retryCount)
    {
        var methodEntry = Cache.GetOrAdd((testMethod, null), tuple =>
        {
            var mtd = tuple.Method;
            var hasMethodAttribute = mtd?.Method.GetCustomAttributes(typeof(RetryFactAttribute)).Any() == true
                                     || mtd?.Method.GetCustomAttributes(typeof(RetryTheoryAttribute)).Any() == true;
            return new RetryEntry(hasMethodAttribute, int.MinValue);
        });

        if (methodEntry.HasRetry)
        {
            retryCount = int.MinValue;
            return false;
        }
        
        var classEntry = Cache.GetOrAdd((null, testMethod.TestClass.Class), tuple =>
        {
            var cls = tuple.Class;
            var retryClassAttribute = cls?.GetCustomAttributes(typeof(RetryClassAttribute)).FirstOrDefault();
            var retryCount = retryClassAttribute.GetRetryCountOrDefault();
            var hasRetry = retryClassAttribute != null && !retryCount.IsDefaultRetryCount();
            return new RetryEntry(hasRetry, retryCount);
        });
        
        retryCount = classEntry.RetryCount;
        return classEntry.HasRetry;
    }

    private readonly struct RetryEntry
    {
        internal bool HasRetry { get; }
        internal int RetryCount { get; }

        internal RetryEntry(bool hasRetry, int retryCount)
        {
            HasRetry = hasRetry;
            RetryCount = retryCount;
        }
    }
}
