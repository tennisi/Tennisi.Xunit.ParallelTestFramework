using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Tests;

public class ParallelTagTests
{
    private readonly ParallelTag _tag;
    
    private static readonly ConcurrentDictionary<ParallelTag, string> TagControl = new ConcurrentDictionary<ParallelTag, string>();
    private static int _counter;
    private readonly ITestOutputHelper _testOutput;
    
    [SuppressMessage("Usage", "xUnit1041:General design", Justification = "General design")]
    public ParallelTagTests(ITestOutputHelper testOutputHelper, ParallelTag parallelTag)
    {
        _tag = parallelTag;
        
        _counter = Interlocked.Increment(ref _counter);
        _testOutput = testOutputHelper;
        _testOutput.WriteLine($"construct: {_counter}:{_tag}");
    }
    
    [Theory]
    [InlineData("p1")]
    [InlineData("p2")]
    public async Task Task1(string p)
    {
        if (p.ToString(CultureInfo.InvariantCulture) == p.ToLower(CultureInfo.InvariantCulture))
        {
            await Test(GetTestMethodName());
        }
    }
    
    [Fact]
    public async Task Task2()
    {
        await Test(GetTestMethodName());
    }
    
    private async Task Test(string who)
    {
        var counter = _counter;
        if (TagControl.TryAdd(_tag, who) == false)
        {
           throw new InvalidOperationException("Tag Control violated");
        }
        _testOutput.WriteLine($"I'm started when {counter} tests are allready created, My name is {who}, my tag {_tag}");
        await Task.Delay(1000);
        var unqState = string.Join(',',TagControl.Select(a => $"{a.Key}-{a.Value}"));
        _testOutput.WriteLine($"I'm finished. Group Control:{unqState}");
    }
   
    private string GetTestMethodName()
    {
        var stackTrace = new StackTrace();
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod() as MethodInfo;
            if (method != null)
            {
                var factAttribute = method.GetCustomAttribute(typeof(FactAttribute), false);
                if (factAttribute != null)
                {
                    return method.Name;
                }
            }
        }
        throw new InvalidOperationException($"Method {_tag} not found");
    }
}