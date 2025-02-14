using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class ParallelSettings
{
    private sealed class TestAsm
    {
        public TestAsm(bool? disable, bool? full, int? degree, ITestFrameworkOptions opts)
        {
            Opts = opts;
            Full = full;
            Disable = disable;
            DegreeOfParallelism = degree;
        }
        internal int? DegreeOfParallelism { get; set; }
        internal bool? Disable {get; set;}
        internal bool? Full {get; set;}
        internal readonly ConcurrentDictionary<string, SemaphoreSlim> LimiterCache = new();
        internal ITestFrameworkOptions Opts { get; set; }
    }
    private static readonly ConcurrentDictionary<string, TestAsm> TestCollectionsCache = new();

    internal static void RefineParallelSetting(AssemblyName assemblyName, ITestFrameworkOptions opts)
    {
        RefineParallelSetting(assemblyName.FullName, opts);
    }
    
    internal static void RefineParallelSetting(string assemblyName, ITestFrameworkOptions opts)
    {
        var behaviour = DetectParallelBehaviour(assemblyName, opts);
        
        if (behaviour is { Full: true, Disable: false })
        {
            opts.SetValue("xunit.discovery.PreEnumerateTheories", true);
            opts.SetValue("xunit.execution.DisableParallelization", false);
            opts.SetValue("xunit.parallelizeTestCollections", true);
            opts.SetValue("xunit.parallelizeAssembly", true);
        }
        else if (behaviour.Disable == true)
        {
            opts.SetValue("xunit.discovery.PreEnumerateTheories", false);
            opts.SetValue("xunit.execution.DisableParallelization", true);
            opts.SetValue("xunit.parallelizeTestCollections", false);
            opts.SetValue("xunit.parallelizeAssembly", false);
        }
    }

    public static bool GetSetting(string assemblyName, string setting)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        var res = TestCollectionsCache.TryGetValue(assemblyName, out var asm);
        if (!res) throw new InvalidOperationException();
        var val = asm != null && asm.Opts.GetValue<bool>(setting);
        return val;
    }
    
    internal static SemaphoreSlim? GetLimiter(string assemblyName, ITestClass testClass)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        
        var res = TestCollectionsCache.TryGetValue(assemblyName, out var asm);
        if (!res) throw new InvalidOperationException();
        int? degreeOfParallelism =  asm?.DegreeOfParallelism;

        var key = assemblyName;
        if (testClass.Class.IsDegreeOfParallelism())
        {
            degreeOfParallelism = testClass.Class.DegreeOfParallelism();;
            key = testClass.Class.Name;
        }
        degreeOfParallelism ??= Environment.ProcessorCount ;
        if (degreeOfParallelism == 0)
            return null;
        
        var result = asm!.LimiterCache.GetOrAdd(key! , _ =>
        {
            var limiter = new SemaphoreSlim(degreeOfParallelism.Value);
            return limiter;
        });
        return result;
    }
    
    private static TestAsm DetectParallelBehaviour(string assemblyName, ITestFrameworkOptions opts)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        return TestCollectionsCache.GetOrAdd(assemblyName , name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var test = assembly.GetCustomAttribute<TestParallelizationAttribute>();
            return new TestAsm(disable: test?.IsDisabledParallelization, full: test?.IsFullParallelization, degree: test?.DegreeOfParallelism, opts: opts);
        });
    }
}