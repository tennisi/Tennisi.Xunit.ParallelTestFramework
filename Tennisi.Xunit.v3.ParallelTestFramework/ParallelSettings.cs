using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Xunit.Sdk;

namespace Tennisi.Xunit;

internal static class ParallelSettings
{
    private sealed class TestAsm
    {
        public TestAsm(bool disbale, bool force, ITestFrameworkOptions opts, string runner)
        {
            Opts = opts;
            Force = force;
            Disable = disbale;
            Runner = runner;
        }
        internal bool Disable {get; set;}
        internal bool Force {get; set;}
        internal ITestFrameworkOptions Opts { get; set; }
        
        internal string Runner { get; set; }
    }
    private static readonly ConcurrentDictionary<string, TestAsm> TestCollectionsCache = new();

    internal static void RefineParallelSetting(AssemblyName assemblyName, ITestFrameworkOptions opts)
    {
        RefineParallelSetting(assemblyName.FullName, opts);
    }
    
    internal static void RefineParallelSetting(string assemblyName, ITestFrameworkOptions opts)
    {
        var behaviour = DetectParallelBehaviour(assemblyName, opts);
        
        if (behaviour.Force && !behaviour.Disable)
        {
            opts.SetValue("xunit.discovery.PreEnumerateTheories", true);
            opts.SetValue("xunit.execution.DisableParallelization", false);
            opts.SetValue("xunit.parallelizeTestCollections", true);
            opts.SetValue("xunit.parallelizeAssembly", true);
        }
        else if (behaviour.Disable)
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

    public static string GetRunner(string assemblyName)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        var res = TestCollectionsCache.TryGetValue(assemblyName, out var asm);
        if (!res) throw new InvalidOperationException();
        return asm!.Runner;
    }
    
    private static TestAsm DetectParallelBehaviour(string assemblyName, ITestFrameworkOptions opts)
    {
        assemblyName = AssemblyInfoExtractor.ExtractNameAndVersion(assemblyName);
        return TestCollectionsCache.GetOrAdd(assemblyName , name =>
        {
            var assembly = Assembly.Load(new AssemblyName(name));
            var force = assembly.GetCustomAttributes(typeof(FullTestParallelizationAttribute), false).Length != 0;
            var disable = assembly.GetCustomAttributes(typeof(DisableTestParallelizationAttribute), false).Length != 0;
            
            var runner = DetectTestRunner(assembly);
            
            return new TestAsm(force: force, disbale:disable, opts: opts, runner: runner);
        });
    }

    private static string DetectTestRunner(Assembly assembly)
    {
        var projectName = Path.GetFileNameWithoutExtension(assembly.Location);
        var result = "";

        var depsFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location)!, $"{projectName}.deps.json");

        if (!File.Exists(depsFilePath))
        {
            return $"{projectName}.deps.json not found";
        }

        var jsonContent = File.ReadAllText(depsFilePath);
        using var doc = JsonDocument.Parse(jsonContent);

        if (doc.RootElement.TryGetProperty("libraries", out var librariesElement))
        {
            var libraries = new Dictionary<string, JsonElement>();

            foreach (var library in librariesElement.EnumerateObject())
            {
                libraries[library.Name] = library.Value;
            }

            var testRunnerPattern = new List<string>
            {
                "xunit.runner.console",
                "xunit.runner.visualstudio",
                "xunit.runner.msbuild",
                "xunit.runner.utility",
                "xunit.runner.reporters",
                "xunit.runner.devices",
                "xunit.runner.stride",
                "xunit.runner.testSuiteInstrumentation",
                "xunit.v3.runner.console",
                "xunit.v3.runner.visualstudio"
            };

            foreach (var library in libraries.Keys)
            {
                if (testRunnerPattern.Any(pattern => library.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    result += library + ";";
                }
            }
        }
        
        if (string.IsNullOrEmpty(result))
            result = "unknown runner";

        return result;
    }
}