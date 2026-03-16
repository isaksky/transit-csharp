using System.Collections.Frozen;
using System.Text.Json;

namespace Transit.Impl;

/// <summary>
/// JSON verbose emitter — writes maps as JSON objects and tags as map entries.
/// </summary>
internal class JsonVerboseEmitter : JsonEmitter
{
    public JsonVerboseEmitter(Utf8JsonWriter jsonWriter, FrozenDictionary<Type, IWriteHandler> handlers, IWriteHandler? defaultWriteHandler = null, Func<object, object>? transform = null)
        : base(jsonWriter, handlers, defaultWriteHandler, transform)
    {
    }

    public override void EmitString(string? prefix, string? tag, string s, bool asDictionaryKey, WriteCache cache)
    {
        var outString = cache.CacheWrite(Util.MaybePrefix(prefix, tag, s), asDictionaryKey);
        if (asDictionaryKey)
            JsonWriter.WritePropertyName(outString);
        else
            JsonWriter.WriteStringValue(outString);
    }

    protected override void EmitTagged(string t, object obj, bool ignored, WriteCache cache)
    {
        EmitDictionaryStart(1L);
        EmitString(Constants.EscTag, t, "", true, cache);
        Marshal(obj, false, cache);
        EmitDictionaryEnd();
    }

    protected override void EmitDictionary(IEnumerable<KeyValuePair<object, object>> keyValuePairs,
        bool ignored, WriteCache cache)
    {
        EmitDictionaryStart(0);
        foreach (var kvp in keyValuePairs)
        {
            Marshal(kvp.Key, true, cache);
            Marshal(kvp.Value, false, cache);
        }
        EmitDictionaryEnd();
    }
}
