using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit.Sdk;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Unit.Tests;

[TestClass]
[SuppressMessage("Maintainability", "CA1515:Рассмотрите возможность сделать общедоступные типы внутренними")]
public class ParallelTestMethodRunnerTests
{
    [TestMethod]
    public async Task ShouldNotExceedMaxConcurrencyWithSyncCases()
    {
        using var cts = new CancellationTokenSource();
        var observer = new Observer();
        observer.StartMonitoring(1, cts.Token);
        const int maxConcurrency = 4;
        const int testCaseCount = 50;
        var testSetting = new TestSettings() { TestDegreeOfParallelism = maxConcurrency };
        ParallelSettings.Instance = testSetting;

        var testCases = new List<IXunitTestCase>();
        for (int i = 0; i < testCaseCount; i++)
        {
            var x = i;
            var cs = new FakeTestCaseSync(i, observer);
            testCases.Add(cs);
        }

        using var msgBus = new FakeMessageBus();

        var testClass = new FakeTestClass();
        var runner = new ParallelTestMethodRunner(
            testMethod: new FakeTestMethod(testClass),
            @class: new ReflectionTypeInfo(testClass.GetType()),
            method: new ReflectionMethodInfo(testClass.GetType().GetMethod(nameof(FakeTestClass.FakeClassAction))!),
            testCases: testCases,
            diagnosticMessageSink: new FakeDiagnosticMessageSink(),
            messageBus: msgBus,
            aggregator: new ExceptionAggregator(),
            cancellationTokenSource: cts,
            constructorArguments: Array.Empty<object>());
        
        var summary = await runner.RunAsync();
        
#if NET8_0
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif
        Assert.AreEqual(0, summary.Failed);
        Assert.IsTrue(observer.TestCases.ToList().Count == testCaseCount);
        Assert.IsTrue(observer.MaxObservedConcurrency <= maxConcurrency, $"Concurrency was {observer.MaxObservedConcurrency}, expected <= {maxConcurrency}");
        Assert.IsTrue(observer.IndependentMaxObservedThreadCount <= maxConcurrency,
            $"Independent Concurrency was {observer.IndependentMaxObservedThreadCount}, expected <= {maxConcurrency}");
    }
    
    [TestMethod]
    public async Task ShouldNotExceedMaxConcurrencyWithAsyncCases()
    {
        using var cts = new CancellationTokenSource();
        var observer = new Observer();
        observer.StartMonitoring(1, cts.Token);
        const int maxConcurrency = 4;
        const int testCaseCount = 50;
        var testSetting = new TestSettings() { TestDegreeOfParallelism = maxConcurrency };
        ParallelSettings.Instance = testSetting;

        var testCases = new List<IXunitTestCase>();
        for (int i = 0; i < testCaseCount; i++)
        {
            var x = i;
            var cs = new FakeTestCaseAsync(i, observer);
            testCases.Add(cs);
        }

        using var msgBus = new FakeMessageBus();

        var testClass = new FakeTestClass();
        var runner = new ParallelTestMethodRunner(
            testMethod: new FakeTestMethod(testClass),
            @class: new ReflectionTypeInfo(testClass.GetType()),
            method: new ReflectionMethodInfo(testClass.GetType().GetMethod(nameof(FakeTestClass.FakeClassAction))!),
            testCases: testCases,
            diagnosticMessageSink: new FakeDiagnosticMessageSink(),
            messageBus: msgBus,
            aggregator: new ExceptionAggregator(),
            cancellationTokenSource: cts,
            constructorArguments: Array.Empty<object>());
        
        var summary = await runner.RunAsync();
        
#if NET8_0
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif
        Assert.AreEqual(0, summary.Failed);
        Assert.IsTrue(observer.TestCases.ToList().Count == testCaseCount);
        Assert.IsTrue(observer.MaxObservedConcurrency <= maxConcurrency, $"Concurrency was {observer.MaxObservedConcurrency}, expected <= {maxConcurrency}");
        Assert.IsTrue(observer.IndependentMaxObservedThreadCount <= maxConcurrency,
            $"Independent Concurrency was {observer.IndependentMaxObservedThreadCount}, expected <= {maxConcurrency}");
    }
}