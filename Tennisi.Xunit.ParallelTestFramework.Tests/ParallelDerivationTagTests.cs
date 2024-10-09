using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Tennisi.Xunit.ParallelTestFramework.Tests;

public class ParallelDerivationTagTests
{
    private const int Attempts = 10000;
    private const int BaseCount = 100;

    [Fact]
    public void ItShouldGenerateTags()
        => Iterate(Attempts, BaseCount, ParallelTag.FromValue);

    [Fact]
    public void ItShouldGenerateDeriviedTags()
        => Iterate(Attempts, BaseCount, x => ParallelTag.FromValue(x).Next());

    [Fact]
    public void ItShouldGenerateTagsAsLong()
        => Iterate(Attempts, BaseCount, x => ParallelTag.FromValue(x).AsLong());

    [Fact]
    public void ItShouldGenerateDerivedTagsAsLong()
        => Iterate(Attempts, BaseCount, x => ParallelTag.FromValue(x).Next().AsLong());

    [Fact]
    public void ItShouldGenerateTagsAsGuid()
        => Iterate(Attempts, BaseCount, x => ParallelTag.FromValue(x).AsGuid());

    [Fact]
    public void ItShouldGenerateDeriveTagsAsGuid()
        => Iterate(Attempts, BaseCount, x => ParallelTag.FromValue(x).Next().AsGuid());

    [Fact]
    public void ItShouldGenerateTagsAsInteger()
        => Iterate(10, 10, x => ParallelTag.FromValue(x).AsInteger());

    [Fact]
    public void ItShouldReseveTcpPortsAndNotReserveDueToOverflow()
    {
        //arrange
        var range = Enumerable.Range(ParallelTag.MinTcpPort, ParallelTag.MaxTcpPort - ParallelTag.MinTcpPort + 1)
            .ToList();
        var ports = new List<int>();
        foreach (var port in range)
        {
            var ptag = ParallelTag.FromValue($"PORT:{port}");
            var tag = ptag.ReserveTcpPort();
            ports.Add(tag);
        }
        
        //assert
        Assert.True(range.SequenceEqual(ports));
        foreach (var tag in ports)
            Assert.InRange(tag, ParallelTag.MinTcpPort, ParallelTag.MaxTcpPort);

        try
        {
            var ptag = ParallelTag.FromValue($"PORT:{ParallelTag.MaxTcpPort+1}");
            ptag.ReserveTcpPort();
            throw new InvalidOperationException("Test should fail");
        }
        catch (Exception e)
        {
            Assert.True(e is InvalidOperationException);
            Assert.True(e.Message == "Maximum number of ports are captured: 65535");
        }
    }

    [Fact]
    public void ItShouldGenerateDerivedTagsAsInteger()
        => Iterate(10, 10, x => ParallelTag.FromValue(x).Next().AsInteger());

    private static Dictionary<T, string> Iterate<T>(int count, int baseCount, Func<string, T>? valueConverter) where T : notnull
    {
        var generatedValues = new Dictionary<T, string>(); 

        for (var baseIndex = 0; baseIndex < baseCount; baseIndex++)
        {
            var baseValue = GenerateBaseValue(baseIndex);
            var generator = new DeterministicGenerator(baseValue);

            for (var i = 0; i < count; i++)
            {
                var value = generator.GenNext(i);
                var convertedValue = valueConverter != null ? valueConverter(value) : (T)Convert.ChangeType(value, typeof(T));

                if (generatedValues.TryGetValue(convertedValue, out var existingBaseValue))
                {
                    throw new InvalidOperationException($"Duplicate value detected: {convertedValue} (already found for base {existingBaseValue}, current base {baseValue})");
                }

                generatedValues[convertedValue] = baseValue;
            }
        }

        if (generatedValues.Count != baseCount * count)
        {
            throw new InvalidOperationException("Not all values were unique.");
        }
        return generatedValues;
    }

    private static string GenerateBaseValue(int index)
    {
        //xUnit.UniqueId
        return index.ToString("x").PadLeft(40, '0').Substring(0, 40);
    }

    private class DeterministicGenerator
    {
        private readonly string _baseValue;

        public DeterministicGenerator(string baseValue)
        {
            _baseValue = baseValue;
        }

        public string GenNext(int index)
        {
            var combined = _baseValue + index;
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}