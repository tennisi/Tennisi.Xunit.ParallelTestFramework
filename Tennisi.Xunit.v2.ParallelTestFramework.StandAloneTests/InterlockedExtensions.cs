using System.Threading;

namespace Tennisi.Xunit.v2.ParallelTestFramework.StandAloneTests;

internal static class InterlockedExtensions
{
    public static void Max(ref int target, int value)
    {
        while (true)
        {
            var current = Volatile.Read(ref target);
            if (value <= current)
                break;

            var original = Interlocked.CompareExchange(ref target, value, current);
            if (original == current)
                break;
        }
    }
}