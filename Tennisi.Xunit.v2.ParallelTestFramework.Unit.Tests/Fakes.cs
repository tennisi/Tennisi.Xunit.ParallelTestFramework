using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit.v2.ParallelTestFramework.Unit.Tests;

internal sealed class FakeMessageBus : IMessageBus
{
    public bool QueueMessage(IMessageSinkMessage message)
    {
        return true;
    }

    public void Dispose() { }
}

internal sealed class FakeTestMethod : ITestMethod
{
    public ITestClass TestClass { get; }
    public IMethodInfo Method => new ReflectionMethodInfo(typeof(FakeTestClass).GetMethod(nameof(FakeTestClass.FakeClassAction))!);
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static IAttributeInfo GetCustomAttribute(Type attributeType) => null;

    public FakeTestMethod(ITestClass testClass)
    {
        TestClass = testClass;
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
    }

    public void Serialize(IXunitSerializationInfo info)
    {
    }
}

internal sealed class FakeTestClass : ITestClass
{
    public ITypeInfo Class => new ReflectionTypeInfo(typeof(FakeTestClass));

    public ITestCollection TestCollection { get; } = new TestCollection(
        new TestAssembly(new ReflectionAssemblyInfo(typeof(FakeTestClass).Assembly)),
        collectionDefinition: null,
        displayName: "FakeCollection"
    );

    public void Deserialize(IXunitSerializationInfo info) { }

    public void Serialize(IXunitSerializationInfo info) { }
    
    public static async Task FakeClassAction()
    {
        await Task.Yield();
    }
}

internal sealed class FakeDiagnosticMessageSink : IMessageSink
{
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Local")] private List<string> Messages { get; } = new();

    public bool OnMessage(IMessageSinkMessage message)
    {
        if (message is IDiagnosticMessage diagnostic)
        {
            Messages.Add(diagnostic.Message);
        }
        return true;
    }
}

internal sealed class FakeTestCase : XunitTestCase
{
    private Observer Observer { get; }
    private int Index { get; }
    public FakeTestCase(int index, Observer observer)
        : base(new FakeDiagnosticMessageSink(), TestMethodDisplay.Method, TestMethodDisplayOptions.None, CreateFakeTestMethod(), Array.Empty<object>())
    {
        Index = index;
        Observer = observer;
    }

    public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        FakeCaseAction();
        return Task.FromResult(new RunSummary { Total = 1, Failed = 0 });
    }

    private static TestMethod CreateFakeTestMethod()
    {
        var methodInfo = typeof(FakeTestCase).GetMethod(nameof(FakeCaseAction))!;
        var typeInfo = new ReflectionTypeInfo(typeof(FakeTestCase));
        var method = new ReflectionMethodInfo(methodInfo);
        var testClass = new TestClass(
            new TestCollection(
                new TestAssembly(new ReflectionAssemblyInfo(typeof(FakeTestCase).Assembly)),
                collectionDefinition: null,
                displayName: "Fake Collection"),
            typeInfo);

        return new TestMethod(testClass, method);
    }
    
    public void FakeCaseAction()
    {
        var concurrent = Interlocked.Increment(ref Observer.RunningThreads);
        Observer.Max(ref Observer.MaxObservedConcurrency, concurrent);

        Thread.Sleep(100);
        
        Interlocked.Decrement(ref Observer.RunningThreads);
        Observer.TestCases.Add(Index);
    }
}