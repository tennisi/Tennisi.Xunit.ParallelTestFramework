using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit;

/// <inheritdoc />
public class ParallelTestAssemblyRunner :
	ParallelTestAssemblyRunnerBase<ParallelTestAssemblyRunnerContext, IXunitTestAssembly, IXunitTestCollection, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestAssemblyRunner"/> class.
	/// </summary>
	protected ParallelTestAssemblyRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="XunitTestAssemblyRunner"/>.
	/// </summary>
	public static ParallelTestAssemblyRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test assembly.
	/// </summary>
	/// <param name="testAssembly">The test assembly to be executed.</param>
	/// <param name="testCases">The test cases associated with the test assembly.</param>
	/// <param name="executionMessageSink">The message sink to send execution messages to.</param>
	/// <param name="executionOptions">The execution options to use when running tests.</param>
	public async ValueTask<RunSummary> Run(
		IXunitTestAssembly testAssembly,
		IReadOnlyCollection<IXunitTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(testAssembly);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(executionMessageSink);
		Guard.ArgumentNotNull(executionOptions);
		
		ParallelSettings.RefineParallelSetting(testAssembly.AssemblyName, executionOptions);

		var runner = ParallelSettings.GetRunner(testAssembly.AssemblyName);
		var seed = Randomizer.Seed;
		executionMessageSink.OnMessage(new DiagnosticMessage($"The test assembly seed: {seed}, runner: {runner}"));

		await using var ctxt = new ParallelTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}

/// <summary>
/// The test assembly runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public class ParallelTestAssemblyRunnerBase<TContext, TTestAssembly, TTestCollection, TTestCase> :
	TestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
		where TContext : ParallelTestAssemblyRunnerBaseContext<TTestAssembly, TTestCase>
		where TTestAssembly : class, IXunitTestAssembly
		where TTestCollection : class, IXunitTestCollection
		where TTestCase : class, IXunitTestCase
{
	/// <inheritdoc/>
	protected override ValueTask<string> GetTestFrameworkDisplayName(TContext ctxt) =>
		new(nameof(Tennisi.Xunit.ParallelTestFramework));

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.AssemblyFixtureMappings.DisposeAsync);
		return await base.OnTestAssemblyFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestAssemblyStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.AssemblyFixtureMappings.InitializeAsync(ctxt.TestAssembly.AssemblyFixtureTypes));
		return result;
	}

	/// <inheritdoc/>
	protected override List<(TTestCollection Collection, List<TTestCase> TestCases)> OrderTestCollections(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCollectionOrderer = ctxt.AssemblyTestCollectionOrderer ?? DefaultTestCollectionOrderer.Instance;
		var testCasesByCollection =
			ctxt.TestCases
				.GroupBy(tc => (TTestCollection)tc.TestCollection, TestCollectionComparer<TTestCollection>.Instance)
				.ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

		IReadOnlyCollection<TTestCollection> orderedTestCollections;

		try
		{
			orderedTestCollections = testCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			ctxt.MessageBus.QueueMessage(new ErrorMessage()
			{
				ExceptionParentIndices = [-1],
				ExceptionTypes = [typeof(TestPipelineException).SafeName()],
				Messages = [
					string.Format(
						CultureInfo.CurrentCulture,
						"Assembly-level test collection orderer '{0}' threw '{1}' during ordering: {2}",
						testCollectionOrderer.GetType().SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message
					)
				],
				StackTraces = [innerEx.StackTrace],
			});

			return [];
		}

		return
			orderedTestCollections
				.Select(collection => (collection, testCasesByCollection[collection]))
				.ToList();
	}

#pragma warning disable CA2012 // We guarantee that parallel ValueTasks are only awaited once

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollections(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.DisableParallelization || exception is not null)
			return await base.RunTestCollections(ctxt, exception);

		ctxt.SetupParallelism();

		Func<Func<ValueTask<RunSummary>>, ValueTask<RunSummary>> taskRunner;
		if (SynchronizationContext.Current is not null)
		{
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			taskRunner = code => new(Task.Factory.StartNew(() => code().AsTask(), ctxt.CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap());
		}
		else
			taskRunner = code => new(Task.Run(() => code().AsTask(), ctxt.CancellationTokenSource.Token));

		List<ValueTask<RunSummary>>? parallel = null;
		List<Func<ValueTask<RunSummary>>>? nonParallel = null;
		var summaries = new List<RunSummary>();

		foreach (var (collection, testCases) in OrderTestCollections(ctxt))
		{
			ValueTask<RunSummary> task() => RunTestCollection(ctxt, collection, testCases);
			if (collection.DisableParallelization)
				(nonParallel ??= []).Add(task);
			else
				(parallel ??= []).Add(taskRunner(task));
		}

		if (parallel?.Count > 0)
			foreach (var task in parallel)
				try
				{
					summaries.Add(await task);
				}
				catch (TaskCanceledException) { }

		if (nonParallel?.Count > 0)
			foreach (var taskFactory in nonParallel)
				try
				{
					summaries.Add(await taskRunner(taskFactory));
					if (ctxt.CancellationTokenSource.IsCancellationRequested)
						break;
				}
				catch (TaskCanceledException) { }

		return new RunSummary()
		{
			Total = summaries.Sum(s => s.Total),
			Failed = summaries.Sum(s => s.Failed),
			NotRun = summaries.Sum(s => s.NotRun),
			Skipped = summaries.Sum(s => s.Skipped),
		};
	}

#pragma warning restore CA2012

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCollection(
		TContext ctxt,
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);

		var testCaseOrderer = ctxt.AssemblyTestCaseOrderer ?? DefaultTestCaseOrderer.Instance;

		return ctxt.RunTestCollection(testCollection, testCases, testCaseOrderer);
	}
}

