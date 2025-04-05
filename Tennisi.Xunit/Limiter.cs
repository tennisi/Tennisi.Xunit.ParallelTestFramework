namespace Tennisi.Xunit;

internal class Limiter
{
    public required TaskScheduler TaskScheduler { get; init; }
    public required TaskFactory TaskFactory{ get; init; }
}