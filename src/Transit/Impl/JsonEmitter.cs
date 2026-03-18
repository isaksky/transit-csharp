using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;

namespace Transit.Net.Impl;

/// <summary>
/// JSON emitter using System.Text.Json Utf8JsonWriter for high-performance
/// binary-to-UTF8 output with no intermediate string allocations.
/// </summary>
internal class JsonEmitter : AbstractEmitter
{
    private static readonly long JsonIntMax = (long)Math.Pow(2, 53) - 1;
    private static readonly long JsonIntMin = -JsonIntMax;

    internal readonly Utf8JsonWriter JsonWriter;

    public JsonEmitter(Utf8JsonWriter jsonWriter, FrozenDictionary<Type, IWriteHandler> handlers, IWriteHandler? defaultWriteHandler = null, Func<object, object>? transform = null)
        : base(handlers, defaultWriteHandler, transform)
    {
        JsonWriter = jsonWriter;
    }

    public override void Emit(object obj, bool asDictionaryKey, WriteCache cache)
        => MarshalTop(obj, cache);

    public override void EmitNull(bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
            EmitString(Constants.EscStr, "_", "", asDictionaryKey, cache);
        else
            JsonWriter.WriteNullValue();
    }

    public override void EmitString(string? prefix, string? tag, string s, bool asDictionaryKey, WriteCache cache)
    {
        var outString = cache.CacheWrite(Util.MaybePrefix(prefix, tag, s), asDictionaryKey);
        JsonWriter.WriteStringValue(outString);
    }

    public override void EmitBoolean(bool b, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
            EmitString(Constants.EscStr, "?", b ? "t" : "f", asDictionaryKey, cache);
        else
            JsonWriter.WriteBooleanValue(b);
    }

    public override void EmitInteger(object i, bool asDictionaryKey, WriteCache cache)
        => EmitInteger(Util.NumberToPrimitiveLong(i), asDictionaryKey, cache);

    public override void EmitInteger(long i, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey || i > JsonIntMax || i < JsonIntMin)
            EmitString(Constants.EscStr, "i", i.ToString(CultureInfo.InvariantCulture), asDictionaryKey, cache);
        else
            JsonWriter.WriteNumberValue(i);
    }

    public override void EmitDouble(object d, bool asDictionaryKey, WriteCache cache)
    {
        if (d is double dbl) EmitDouble(dbl, asDictionaryKey, cache);
        else if (d is float flt) EmitDouble(flt, asDictionaryKey, cache);
        else throw new TransitException("Unknown double type: " + d.GetType());
    }

    public override void EmitDouble(float d, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
            EmitString(Constants.EscStr, "d", d.ToString(CultureInfo.InvariantCulture), asDictionaryKey, cache);
        else
            JsonWriter.WriteNumberValue(d);
    }

    public override void EmitDouble(double d, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
            EmitString(Constants.EscStr, "d", d.ToString(CultureInfo.InvariantCulture), asDictionaryKey, cache);
        else
            JsonWriter.WriteNumberValue(d);
    }

    public override void EmitBinary(object b, bool asDictionaryKey, WriteCache cache)
        => EmitString(Constants.EscStr, "b", Convert.ToBase64String((byte[])b), asDictionaryKey, cache);

    public override void EmitListStart(long size) => JsonWriter.WriteStartArray();
    public override void EmitListEnd() => JsonWriter.WriteEndArray();
    public override void EmitDictionaryStart(long size) => JsonWriter.WriteStartObject();
    public override void EmitDictionaryEnd() => JsonWriter.WriteEndObject();

    public override void FlushWriter() => JsonWriter.Flush();
    public override bool PrefersStrings() => true;

    protected override void EmitDictionary(IEnumerable<KeyValuePair<object, object>> keyValuePairs,
        bool ignored, WriteCache cache)
    {
        EmitListStart(0);
        EmitString(null, null, Constants.DirectoryAsList, false, cache);

        foreach (var kvp in keyValuePairs)
        {
            Marshal(kvp.Key, true, cache);
            Marshal(kvp.Value, false, cache);
        }

        EmitListEnd();
    }
}
