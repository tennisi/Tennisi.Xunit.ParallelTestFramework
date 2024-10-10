using System.Security.Cryptography;
using System.Text;
using Xunit.Sdk;

namespace Tennisi.Xunit
{
    public readonly struct ParallelTag : IEquatable<ParallelTag>
    {
        private readonly string _value;
        private readonly int _next;
        private readonly int _indexInConstructor;

        internal static ParallelTag? FromTestCase(object[]? constructorArguments, IXunitTestCase testCase)
        {
            if (constructorArguments == null)
                return null;

            for (var i = 0; i < constructorArguments.Length; i++)
            {
                if (constructorArguments[i] is ParallelTag)
                {
                    var result = new ParallelTag(testCase.UniqueID, i, 0);
                    return result;
                }
            }

            return null;
        }

        internal static void Inject(ref ParallelTag? tag, ref object[] args)
        {
            if (tag == null)
                throw new InvalidOperationException(nameof(ParallelTag));

            args[tag.Value._indexInConstructor] = tag;
        }

        private ParallelTag(string value, int indexInConstructor, int next)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));

            _value = value;
            _indexInConstructor = indexInConstructor;
            _next = next;
        }

        public override string ToString() => $"{_value}:{_next}";

        public ParallelTag Next(int increment = 1)
        {
            return new ParallelTag(_value, 0, _next + increment);
        }

        public int AsInteger()
        {
            return HashCode.Combine(_value, _next);
        }

        public long AsLong()
        {
            long valueHash = 0;
            foreach (var c in _value)
            {
                valueHash = (valueHash * 31) + c;
            }
            return (valueHash ^ _next) & long.MaxValue;
        }

        public Guid AsGuid()
        {
            var valueBytes = Encoding.UTF8.GetBytes(_value);
            var nextBytes = BitConverter.GetBytes(_next);
            var combinedBytes = new byte[valueBytes.Length + nextBytes.Length];
            Buffer.BlockCopy(valueBytes, 0, combinedBytes, 0, valueBytes.Length);
            Buffer.BlockCopy(nextBytes, 0, combinedBytes, valueBytes.Length, nextBytes.Length);
            var hashBytes = SHA256.HashData(combinedBytes);
            var guidBytes = new byte[16];
            Array.Copy(hashBytes, guidBytes, 16);
            return new Guid(guidBytes);
        }

        public bool Equals(ParallelTag other)
        {
            return _value == other._value && _next == other._next;
        }

        public override bool Equals(object obj)
        {
            return obj is ParallelTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value, _next);
        }

        public static bool operator ==(ParallelTag left, ParallelTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ParallelTag left, ParallelTag right)
        {
            return !(left == right);
        }

        public static ParallelTag FromValue(string value, int next = 0)
        {
            return new ParallelTag(value, 0, next);
        }
    }
}