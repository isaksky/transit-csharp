namespace Transit.Net.Java;

/// <summary>
/// Conversion utilities between .NET DateTime and Java epoch milliseconds.
/// </summary>
internal static class Convert
{
    private static readonly DateTime JavaEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Converts a DateTime to Java epoch milliseconds.
    /// </summary>
    public static long ToJavaTime(DateTime dateTime)
    {
        var dto = new DateTimeOffset(dateTime);
        return (long)(dto.UtcDateTime - JavaEpoch).TotalMilliseconds;
    }

    /// <summary>
    /// Converts Java epoch milliseconds to a DateTime.
    /// </summary>
    public static DateTime FromJavaTime(long javaTime)
    {
        return JavaEpoch.AddMilliseconds(javaTime).ToLocalTime();
    }
}
