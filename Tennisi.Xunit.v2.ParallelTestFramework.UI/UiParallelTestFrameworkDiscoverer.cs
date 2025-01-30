using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tennisi.Xunit;

internal sealed class UiParallelTestFrameworkDiscoverer: ParallelTestFrameworkDiscoverer
{
    public UiParallelTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IXunitTestCollectionFactory? collectionFactory = null) : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
    {
    }
}