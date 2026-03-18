namespace Transit.Net;

/// <summary>
/// Represents a transit exception.
/// </summary>
public class TransitException : Exception
{
    public TransitException(string message) : base(message) { }
    public TransitException(string message, Exception innerException) : base(message, innerException) { }
}
