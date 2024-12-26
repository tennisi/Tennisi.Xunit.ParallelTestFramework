using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class ParallelTestClassRunner : XunitTestClassRunner
{
    private readonly bool _disableTestParallelizationOnAssembly;

    /// <inheritdoc />
    public ParallelTestClassRunner(ITestClass testClass,
        IReflectionTypeInfo @class,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        IDictionary<Type, object> collectionFixtureMappings)
        : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
    {
        _disableTestParallelizationOnAssembly = 
            ParallelSettings.GetSetting(@Class.Assembly.Name, "xunit.execution.DisableParallelization");
    }

    /// <summary>
    /// Creates a custom test method runner for executing test methods in parallel.
    /// </summary>
    /// <param name="testMethod">The test method to be executed.</param>
    /// <param name="method">Reflection information about the test method.</param>
    /// <param name="testCases">The collection of test cases to execute with the method.</param>
    /// <param name="constructorArguments">Arguments to be passed to the test class constructor.</param>
    /// <returns>
    /// A <see cref="ParallelTestMethodRunner"/> instance responsible for executing the test method.
    /// </returns>
    /// <remarks>
    /// This method can be overridden to provide a custom implementation of the test method runner,
    /// enabling specialized behavior for test execution.
    /// </remarks>
    protected virtual ParallelTestMethodRunner CreateMethodRunner(ITestMethod testMethod,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        object?[] constructorArguments)
    {
        return new ParallelTestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink,
            MessageBus,
            new ExceptionAggregator(Aggregator),
            CancellationTokenSource, constructorArguments);
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        object?[] constructorArguments)
    {
        var inst = CreateMethodRunner(testMethod, method, testCases, constructorArguments);
        var result = inst.RunAsync();
        return result;
    }

    /// <inheritdoc />
    protected override async Task<RunSummary> RunTestMethodsAsync()
    {
        var disableParallelizationAttribute = TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any();

        var disableParallelizationOnCustomCollection = TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any()
                                                       && !TestClass.Class.GetCustomAttributes(typeof(EnableParallelizationAttribute)).Any();

        var disableParallelization = _disableTestParallelizationOnAssembly || disableParallelizationAttribute || disableParallelizationOnCustomCollection;

        if (disableParallelization)
            return await base.RunTestMethodsAsync().ConfigureAwait(false);

        var summary = new RunSummary();
        IEnumerable<IXunitTestCase> orderedTestCases;
        try
        {
            orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
        }
        catch (Exception ex)
        {
            var innerEx = Unwrap(ex);
            DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Test case orderer '{TestCaseOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
            orderedTestCases = TestCases.ToList();
        }

        var constructorArguments = CreateTestClassConstructorArguments();
        var methodGroups = orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance);
        var methodTasks = methodGroups.Select(m => RunTestMethodAsync(m.Key, (IReflectionMethodInfo)m.Key.Method, m, constructorArguments));
        var methodSummaries = await Task.WhenAll(methodTasks).ConfigureAwait(false);

        foreach (var methodSummary in methodSummaries)
        {
            summary.Aggregate(methodSummary);
        }

        return summary;
    }

    /// <inheritdoc />
    protected override object?[] CreateTestClassConstructorArguments()
    {
        var isStaticClass = Class.Type.GetTypeInfo().IsAbstract && Class.Type.GetTypeInfo().IsSealed;
        if (!isStaticClass)
        {
            var ctor = SelectTestClassConstructor();
            if (ctor != null)
            {
                var unusedArguments = new List<Tuple<int, ParameterInfo>>();
                var parameters = ctor.GetParameters();

                object?[] constructorArguments = new object?[parameters.Length];
                for (var idx = 0; idx < parameters.Length; ++idx)
                {
                    var parameter = parameters[idx];
                    object? argumentValue;

                    if (parameter.ParameterType == typeof(ParallelTag))
                    {
                        constructorArguments[idx] = new ParallelTag();
                    }
                    else if (TryGetConstructorArgument(ctor, idx, parameter, out argumentValue))
                        constructorArguments[idx] = argumentValue;
                    else if (parameter.HasDefaultValue)
                        constructorArguments[idx] = parameter.DefaultValue;
                    else if (parameter.IsOptional)
                        constructorArguments[idx] = GetDefaultValue(parameter.ParameterType.GetTypeInfo());
                    else if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                        constructorArguments[idx] = Array.CreateInstance(parameter.ParameterType, 0);
                    else
                        unusedArguments.Add(Tuple.Create(idx, parameter));
                }

                if (unusedArguments.Count > 0)
                    Aggregator.Add(new TestClassException(FormatConstructorArgsMissingMessage(ctor, unusedArguments)));

                return constructorArguments;
            }
        }
        return Array.Empty<object>();
    }

    private static object? GetDefaultValue(TypeInfo typeInfo)
    {
        if (typeInfo.IsValueType)
            return Activator.CreateInstance(typeInfo.AsType());

        return null;
    }

    private static Exception Unwrap(Exception ex)
    {
        while (true)
        {
            if (ex is not TargetInvocationException tiex || tiex.InnerException == null)
                return ex;

            ex = tiex.InnerException;
        }
    }
}