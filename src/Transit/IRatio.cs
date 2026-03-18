using System.Numerics;

namespace Transit.Net;

/// <summary>
/// Represents a ratio (numerator/denominator) of BigIntegers.
/// </summary>
public interface IRatio
{
    /// <summary>Gets the numerator.</summary>
    BigInteger Numerator { get; }

    /// <summary>Gets the denominator.</summary>
    BigInteger Denominator { get; }

    /// <summary>Gets the double value of the ratio.</summary>
    double GetValue();
}
