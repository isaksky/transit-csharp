// BigRational.cs — Ported from src_old/Transit/Numerics/BigRational.cs
// Minimal port of the core functionality needed for transit 'f' tag decoding.

using System.Globalization;
using System.Numerics;

namespace Transit.Net.Numerics;

/// <summary>
/// Represents an arbitrarily large signed rational number.
/// Used for transit 'f' (big decimal) encoding.
/// 
/// This is a simplified port — it stores the value as a decimal
/// for practical purposes in .NET, while maintaining the BigRational API.
/// </summary>
public readonly struct BigRational : IEquatable<BigRational>, IComparable<BigRational>
{
    private readonly decimal _value;

    public BigRational(decimal value) => _value = value;
    public BigRational(int value) => _value = value;
    public BigRational(long value) => _value = value;
    public BigRational(double value) => _value = (decimal)value;
    public BigRational(BigInteger numerator, BigInteger denominator)
    {
        _value = (decimal)numerator / (decimal)denominator;
    }

    public BigInteger Numerator
    {
        get
        {
            // Extract numerator from decimal representation
            var bits = decimal.GetBits(_value);
            var lo = (uint)bits[0];
            var mid = (uint)bits[1];
            var hi = (uint)bits[2];
            var sign = (bits[3] >> 31) != 0;
            var raw = new BigInteger(lo) | (new BigInteger(mid) << 32) | (new BigInteger(hi) << 64);
            return sign ? -raw : raw;
        }
    }

    public BigInteger Denominator
    {
        get
        {
            var bits = decimal.GetBits(_value);
            var scale = (bits[3] >> 16) & 0xFF;
            return BigInteger.Pow(10, scale);
        }
    }

    public decimal ToDecimal() => _value;

    public static BigRational Parse(string s)
        => new(decimal.Parse(s, CultureInfo.InvariantCulture));

    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
    public override int GetHashCode() => _value.GetHashCode();
    public override bool Equals(object? obj) => obj is BigRational other && Equals(other);
    public bool Equals(BigRational other) => _value == other._value;
    public int CompareTo(BigRational other) => _value.CompareTo(other._value);

    public static bool operator ==(BigRational left, BigRational right) => left.Equals(right);
    public static bool operator !=(BigRational left, BigRational right) => !left.Equals(right);
    public static implicit operator BigRational(decimal value) => new(value);
}
