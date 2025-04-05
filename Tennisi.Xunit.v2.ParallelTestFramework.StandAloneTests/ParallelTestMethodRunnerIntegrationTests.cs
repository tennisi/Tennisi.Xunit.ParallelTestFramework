using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit.Sdk;

namespace Tennisi.Xunit.v2.ParallelTestFramework.StandAloneTests;

[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
public class ParallelTestMethodRunnerIntegrationTests
{
    [TestMethod]
    [Ignore]
    public async Task Should_Not_Exceed_Max_Concurrency()
    {
        const int maxConcurrency = 4;
        const int testCaseCount = 20;
        var testSetting = new TestSettings() { TestDegreeOfParallelism = maxConcurrency };
        ParallelSettings.Instance = testSetting;
        var runningThreads = 0;
        var maxObservedConcurrency = 0;

        var testCases = new List<IXunitTestCase>();
        for (int i = 0; i < testCaseCount; i++)
        {
            testCases.Add(new FakeTestCase(FakeTestClass.FakeTestMethod, () =>
            {
                var concurrent = Interlocked.Increment(ref runningThreads);
                InterlockedExtensions.Max(ref maxObservedConcurrency, concurrent);

                Thread.Sleep(100); // simulate work
                Interlocked.Decrement(ref runningThreads);
            }));
        }

        using var msgBus = new FakeMessageBus();
        using var cts = new CancellationTokenSource();

        var testClass = new FakeTestClass();
        var runner = new ParallelTestMethodRunner(
            testMethod: new FakeTestMethod(testClass),
            @class: new ReflectionTypeInfo(testClass.GetType()),
            method: new ReflectionMethodInfo(testClass.GetType().GetMethod(nameof(FakeTestClass.FakeTestMethod))!),
            testCases: testCases,
            diagnosticMessageSink: new NullMessageSink(),
            messageBus: msgBus,
            aggregator: new ExceptionAggregator(),
            cancellationTokenSource: cts,
            constructorArguments: Array.Empty<object>());
        
        var summary = await runner.RunAsync();

        Assert.AreEqual(0, summary.Failed);
        Assert.IsTrue(maxObservedConcurrency <= maxConcurrency, $"Concurrency was {maxObservedConcurrency}, expected <= {maxConcurrency}");
    }
}