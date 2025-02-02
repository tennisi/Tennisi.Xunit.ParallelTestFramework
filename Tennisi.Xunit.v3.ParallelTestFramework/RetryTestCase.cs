using Xunit.v3;

namespace Tennisi.Xunit;

[Serializable]
internal sealed class RetryTestCase : XunitTestCase
{
    private int _retryCount;
    private IXunitTestCase _realCase;

#pragma warning disable CS0618 // Type or member is obsolete
    public RetryTestCase(IXunitTestCase baseCase, int retryCount)
    {
        _realCase = baseCase;
        _retryCount = retryCount;
    }
#pragma warning restore CS0618 // Type or member is obsolete
}