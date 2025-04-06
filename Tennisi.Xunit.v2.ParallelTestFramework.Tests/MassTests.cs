using Xunit;
using Xunit.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Tests;

public class MassiveTests
{
    [SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows")]
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public abstract class MassTests<T>
    {
        private const int TestCount = 100;

        private static readonly TimeSpan Chop = TimeSpan.FromSeconds(0.01);

        private static readonly DateTime StartTime;
        private static int Count;
        internal static bool Done;
        internal static double Duration;

        static MassTests()
        {
            StartTime = DateTime.Now;
        }

        private static async Task Log(int index)
        {
            await Task.Delay(Chop.Add(new TimeSpan(0, 0, 0, 0, index)));
            Interlocked.Increment(ref Count);
            if (Count == TestCount)
            {
                Duration = DateTime.Now.Subtract(StartTime).TotalMilliseconds;
                Done = true;
            }
        }

        private static async Task Test(int index)
        {
            await Log(index);
        }

        public static IEnumerable<object[]> GenerateTestData()
        {
            var random = new Random();
            var testData = new List<object[]>();

            for (var i = 1; i <= TestCount; i++)
            {
                testData.Add(new object[] { i });
            }

            return testData;
        }

        [Theory]
        [MemberData(nameof(GenerateTestData))]
        public async Task TestGeneratedData(object index)
        {
            await Log((int)index);
        }
    }
    
    public class TennisDefaultByCoreCount
    {
        
    }

    
    public class XUnitDefault
    {
    }
    
    public class LimitedByOneCore
    {
    }
    
    
    public class MassTestTennisiDefaultByCoreCount : MassTests<TennisDefaultByCoreCount>
    {
    }
    
    [TestParallelization(enabled: "true", degreeOfParallelism: "0" )]
    public class MassTestXUnitDefault: MassTests<XUnitDefault>
    {
    }

    [TestParallelization(enabled: "true", degreeOfParallelism: "1" )]
    public class MassTestLimitedByOneCore: MassTests<LimitedByOneCore>
    {
    }
    
    
    public class MassTestsWatcher
    {
        private readonly ITestOutputHelper _testOutput;

        public MassTestsWatcher(ITestOutputHelper testOutputHelper)
        {
            _testOutput = testOutputHelper;
        }

        [Fact]
        public async Task Test()
        {
            while (!MassTestTennisiDefaultByCoreCount.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            var massTestTennisiDefaultByCoreCount = MassTestTennisiDefaultByCoreCount.Duration;
            _testOutput.WriteLine($"MassTestTennsiDefaultByCoreCount: {massTestTennisiDefaultByCoreCount} msec");

                    
            while (!MassTestXUnitDefault.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            var massTestXUnitDefault = MassTestXUnitDefault.Duration;
            _testOutput.WriteLine($"MassTestXUnitDefault: {massTestXUnitDefault} msec");
            
            
            while (!MassTestLimitedByOneCore.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            var massTestLimitedByOneCore = MassTestLimitedByOneCore.Duration;
            _testOutput.WriteLine($"MassTestLimitedByOneCore: {massTestLimitedByOneCore} msec");

            Assert.True(Math.Abs(massTestXUnitDefault - massTestTennisiDefaultByCoreCount) <= 0.2 * massTestXUnitDefault);
        }
    }
}