using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit.v2.ParallelTestFramework.StandAloneTests;

internal sealed class FakeMessageBus : IMessageBus
{
    public bool QueueMessage(IMessageSinkMessage message)
    {
        // Accept all messages and do nothing
        return true;
    }

    public void Dispose() { }
}

internal sealed class FakeTestMethod : ITestMethod
{
    public ITestClass TestClass { get; }
    public IMethodInfo Method => new ReflectionMethodInfo(typeof(FakeTestClass).GetMethod(nameof(FakeTestClass.FakeTestMethod))!);
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
    public ITestCollection TestCollection => new TestCollection(null!, null!, "Fake");
    public static void FakeTestMethod() { }
    
    public void Deserialize(IXunitSerializationInfo info)
    {
    }

    public void Serialize(IXunitSerializationInfo info)
    {
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
    private readonly Action _action;

    public FakeTestCase(Action action)
        : base(new FakeDiagnosticMessageSink(), TestMethodDisplay.Method, TestMethodDisplayOptions.None, CreateFakeTestMethod(), Array.Empty<object>())
    {
        _action = action;
    }

    public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        return Task.Run(() =>
        {
            _action?.Invoke();
            return new RunSummary { Total = 1, Failed = 0 };
        });
    }

    private static ITestMethod CreateFakeTestMethod()
    {
        var methodInfo = typeof(FakeTestCase).GetMethod(nameof(DummyTestMethod), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var typeInfo = Reflector.Wrap(typeof(FakeTestCase));
        var method = Reflector.Wrap(methodInfo);
        return new TestMethod(new TestClass(typeInfo), method);
    }

    private static void DummyTestMethod() { }
}