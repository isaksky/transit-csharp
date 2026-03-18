using System.Numerics;

namespace Transit.Net.Impl;

/// <summary>
/// Represents a ratio (numerator/denominator) of BigIntegers.
/// </summary>
internal sealed class Ratio : IRatio, IEquatable<Ratio>
{
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public Ratio(BigInteger numerator, BigInteger denominator)
    {
        Numerator = numerator;
        Denominator = denominator;
    }

    public double GetValue() => (double)Numerator / (double)Denominator;

    public override bool Equals(object? obj) => obj is Ratio other && Equals(other);
    public bool Equals(Ratio? other) =>
        other is not null && Numerator == other.Numerator && Denominator == other.Denominator;
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    public override string ToString() => $"{Numerator}/{Denominator}";
}
