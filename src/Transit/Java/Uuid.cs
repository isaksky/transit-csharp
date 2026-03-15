namespace Transit.Java;

/// <summary>
/// Wrapper providing Java-style UUID most/least significant bits access for Guid.
/// </summary>
internal readonly struct Uuid
{
    public long MostSignificantBits { get; }
    public long LeastSignificantBits { get; }

    private Uuid(long msb, long lsb)
    {
        MostSignificantBits = msb;
        LeastSignificantBits = lsb;
    }

    /// <summary>
    /// Converts a Guid to a Uuid with Java-compatible byte ordering.
    /// </summary>
    public static explicit operator Uuid(Guid guid)
    {
        byte[] bytes = guid.ToByteArray();

        // .NET Guid byte order differs from Java UUID for the first 8 bytes
        // .NET: int32 (LE), int16 (LE), int16 (LE), 8 bytes (BE)
        // Java: int32 (BE), int16 (BE), int16 (BE), 8 bytes (BE)
        long msb = ((long)bytes[3] << 56) | ((long)bytes[2] << 48) |
                   ((long)bytes[1] << 40) | ((long)bytes[0] << 32) |
                   ((long)bytes[5] << 24) | ((long)bytes[4] << 16) |
                   ((long)bytes[7] << 8)  | ((long)bytes[6]);

        long lsb = ((long)bytes[8] << 56)  | ((long)bytes[9] << 48) |
                   ((long)bytes[10] << 40) | ((long)bytes[11] << 32) |
                   ((long)bytes[12] << 24) | ((long)bytes[13] << 16) |
                   ((long)bytes[14] << 8)  | ((long)bytes[15]);

        return new Uuid(msb, lsb);
    }

    /// <summary>
    /// Converts a Uuid back to a .NET Guid.
    /// </summary>
    public static Guid ToGuid(long msb, long lsb)
    {
        byte[] bytes = new byte[16];

        // Reverse the LE swaps for the first 8 bytes
        bytes[3] = (byte)(msb >> 56);
        bytes[2] = (byte)(msb >> 48);
        bytes[1] = (byte)(msb >> 40);
        bytes[0] = (byte)(msb >> 32);
        bytes[5] = (byte)(msb >> 24);
        bytes[4] = (byte)(msb >> 16);
        bytes[7] = (byte)(msb >> 8);
        bytes[6] = (byte)(msb);

        bytes[8]  = (byte)(lsb >> 56);
        bytes[9]  = (byte)(lsb >> 48);
        bytes[10] = (byte)(lsb >> 40);
        bytes[11] = (byte)(lsb >> 32);
        bytes[12] = (byte)(lsb >> 24);
        bytes[13] = (byte)(lsb >> 16);
        bytes[14] = (byte)(lsb >> 8);
        bytes[15] = (byte)(lsb);

        return new Guid(bytes);
    }

    public override string ToString()
    {
        var guid = ToGuid(MostSignificantBits, LeastSignificantBits);
        return guid.ToString();
    }
}
