using Xunit.Sdk;
using Xunit.v3;

namespace Tennisi.Xunit.v3;

/// <inheritdoc />
internal class ParallelTestFrameworkDiscoverer: XunitTestFrameworkDiscoverer
{
    public ParallelTestFrameworkDiscoverer(
        IXunitTestAssembly testAssembly,
        IXunitTestCollectionFactory? collectionFactory = null) :
        base(testAssembly, collectionFactory)
    {
    }
    
    protected override ValueTask<bool> FindTestsForType(IXunitTestClass testClass, ITestFrameworkDiscoveryOptions discoveryOptions,
        Func<ITestCase, ValueTask<bool>> discoveryCallback)
    {
        ParallelSettings.RefineParallelSetting(TestAssembly.AssemblyName, discoveryOptions);
        return base.FindTestsForType(testClass, discoveryOptions, discoveryCallback);
    }

    private ValueTask<bool> DiscoveryCallback(ITestCase arg)
    {
        return new ValueTask<bool>(false);
    }

    protected override ValueTask<bool> FindTestsForMethod(IXunitTestMethod testMethod, ITestFrameworkDiscoveryOptions discoveryOptions,
        Func<ITestCase, ValueTask<bool>> discoveryCallback)
    {
      
      //  ParallelSettings.RefineParallelSetting(TestAssembly.AssemblyName, discoveryOptions);
        
      //  if (!testMethod.ShouldUseClassRetry(out var retryCount))
            return base.FindTestsForMethod(testMethod, discoveryOptions, discoveryCallback);

     //   return FindTestsForMethod2(testMethod, includeSourceInformation, messageBus, discoveryOptions, retryCount);
    }
    /*private bool FindTestsForMethod2(IXunitTestMethod testMethod, ITestFrameworkDiscoveryOptions discoveryOptions,
        Func<ITestCase, ValueTask<bool>> discoveryCallback, int retryCount)
    {
        var factAttributes = testMethod.FactAttributes;
        if (factAttributes.Count > 1)
        {
            var message = string.Format(CultureInfo.CurrentCulture,
                "Test method '{0}.{1}' has multiple [Fact]-derived attributes", testMethod.TestClass.Class.Name,
                testMethod.Method.Name);
            using var testCase = new ExecutionErrorTestCase(DiagnosticMessageSink, TestMethodDisplay.ClassAndMethod,
                TestMethodDisplayOptions.None, testMethod, message);
                return ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
        }

        var factAttribute = factAttributes.FirstOrDefault();
        if (factAttribute == null)
            return true;

        var factAttributeType = (factAttribute as IReflectionAttributeInfo)?.Attribute.GetType();

        Type? discovererType = null;
        if (factAttributeType == null || !DiscovererTypeCache.TryGetValue(factAttributeType, out discovererType))
        {
            var testCaseDiscovererAttribute =
                factAttribute.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute)).FirstOrDefault();
            if (testCaseDiscovererAttribute != null)
            {
                var args = testCaseDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                discovererType = SerializationHelper.GetType(args[1], args[0]);
            }

            if (factAttributeType != null)
                DiscovererTypeCache[factAttributeType] = discovererType;
        }

        if (discovererType == null)
            return true;

        var discoverer = GetDiscoverer(discovererType);
        if (discoverer == null)
            return true;

        foreach (var testCase in discoverer.Discover(discoveryOptions, testMethod, factAttribute))
        {
            var retryTestCase = new RetryTestCase(testCase, retryCount);
            if (!ReportDiscoveredTestCase(retryTestCase, includeSourceInformation, messageBus))
                return false;
        }

        return true;
    }*/


}