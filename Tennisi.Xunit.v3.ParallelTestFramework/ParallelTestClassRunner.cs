using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/// <inheritdoc />
internal class ParallelTestClassRunner : XunitTestClassRunnerBase<ParallelTestClassRunnerContext, IXunitTestClass,
	IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	protected ParallelTestClassRunner()
	{
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	public static ParallelTestClassRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	/// <returns></returns>
	public async ValueTask<RunSummary> Run(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new ParallelTestClassRunnerContext(testClass, @testCases, explicitOption, messageBus,
			testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings);
		await ctxt.InitializeAsync();

		return await ctxt.Aggregator.RunAsync(() => Run(ctxt), default);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestMethod(
		ParallelTestClassRunnerContext ctxt,
		IXunitTestMethod? testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		object?[] constructorArguments)
	{
		Guard.ArgumentNotNull(ctxt);

		// Technically not possible because of the design of TTestClass, but this signature is imposed
		// by the base class, which allows method-less tests
		return
			testMethod is null
				? new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, testCases,
					"Test case '{0}' does not have an associated method and cannot be run by XunitTestMethodRunner",
					sendTestMethodMessages: true))
				: ParallelTestMethodRunner.Instance.Run(
					testMethod,
					testCases,
					ctxt.ExplicitOption,
					ctxt.MessageBus,
					ctxt.Aggregator.Clone(),
					ctxt.CancellationTokenSource,
					constructorArguments
				);
	}

	protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(ParallelTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.Aggregator.HasExceptions)
		{
			var ctor = SelectTestClassConstructor(ctxt);
			if (ctor is not null)
			{
				var unusedArguments = new List<Tuple<int, ParameterInfo>>();
				var parameters = ctor.GetParameters();

				var constructorArguments = new object?[parameters.Length];
				for (var idx = 0; idx < parameters.Length; ++idx)
				{
					var parameter = parameters[idx];

					var argumentValue = await GetConstructorArgument(ctxt, ctor, idx, parameter);
					if (parameter.ParameterType == typeof(ParallelTag))
					{
						constructorArguments[idx] = new ParallelTag();
					}
					else if (argumentValue is not null)
						constructorArguments[idx] = argumentValue;
					else if (parameter.HasDefaultValue)
						constructorArguments[idx] = parameter.DefaultValue;
					else if (parameter.IsOptional)
						constructorArguments[idx] = parameter.ParameterType.GetDefaultValue();
					else if (parameter.GetCustomAttribute<ParamArrayAttribute>() is not null)
						constructorArguments[idx] = Array.CreateInstance(parameter.ParameterType, 0);
					else
						unusedArguments.Add(Tuple.Create(idx, parameter));
				}

				if (unusedArguments.Count > 0)
					ctxt.Aggregator.Add(new TestPipelineException(FormatConstructorArgsMissingMessage(ctxt, ctor, unusedArguments)));

				return constructorArguments;
			}
		}

		return [];
	}
}