using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Abstractions;

namespace Tennisi.Xunit;

internal static class ParallelSettings
{
    private sealed class TestAsm
    {
        public TestAsm(bool? disable, bool? full, ITestFrameworkOptions opts)
        {
            Opts = opts;
            Full = full;
            Disable = disable;
        }
        internal bool? Disable {get; set;}
        internal bool? Full {get; set;}
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
    
    private static TestAsm DetectParallelBehaviour(string assemblyName, ITestFrameworkOptions opts)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        return TestCollectionsCache.GetOrAdd(assemblyName , name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var test = assembly.GetCustomAttribute<TestParallelizationAttribute>();
            return new TestAsm(disable: test?.IsDisabledParallelization, full: test?.IsFullParallelization, opts: opts);
        });
    }
}