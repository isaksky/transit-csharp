using System.Collections.Frozen;
using System.Text.Json;

namespace Transit.Impl;

/// <summary>
/// JSON parser using System.Text.Json. Uses JsonDocument to parse the stream
/// into a DOM, then walks the JsonElement tree. This avoids the complexity of
/// managing the ref struct Utf8JsonReader across method boundaries while still
/// getting the performance benefits of System.Text.Json's UTF-8 native parsing.
/// </summary>
internal sealed class JsonParser : AbstractParser
{
    private readonly JsonElement _root;

    public JsonParser(
        JsonElement root,
        FrozenDictionary<string, IReadHandler> handlers,
        IDefaultReadHandler<object>? defaultHandler,
        IDictionaryReader dictionaryBuilder,
        IListReader listBuilder)
        : base(handlers, defaultHandler, dictionaryBuilder, listBuilder)
    {
        _root = root;
    }

    /// <summary>
    /// Formats a DateTime for verbose transit encoding.
    /// </summary>
    public new static string FormatDateTime(DateTime value) => AbstractParser.FormatDateTime(value);

    public override object? Parse(ReadCache cache)
    {
        return ParseElement(_root, false, cache);
    }

    public override object? ParseVal(bool asDictionaryKey, ReadCache cache)
    {
        return ParseElement(_root, asDictionaryKey, cache);
    }

    private object? ParseElement(JsonElement element, bool asDictionaryKey, ReadCache cache)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => ParseNumber(element),
            JsonValueKind.String => cache.CacheRead(element.GetString()!, asDictionaryKey, this),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => ParseArrayElement(element, asDictionaryKey, cache, null),
            JsonValueKind.Object => ParseObjectElement(element, asDictionaryKey, cache, null),
            _ => null,
        };
    }

    private static object ParseNumber(JsonElement element)
    {
        if (element.TryGetInt64(out var l))
            return l;
        if (element.TryGetDouble(out var d))
            return d;
        return element.GetDecimal();
    }

    public override object? ParseDictionary(bool ignored, ReadCache cache, IDictionaryReadHandler? handler)
    {
        return ParseObjectElement(_root, ignored, cache, handler);
    }

    private object? ParseObjectElement(JsonElement element, bool ignored, ReadCache cache, IDictionaryReadHandler? handler)
    {
        var dr = handler?.DictionaryReader() ?? DictionaryBuilder;
        var d = dr.Init();

        foreach (var prop in element.EnumerateObject())
        {
            var key = cache.CacheRead(prop.Name, true, this);
            if (key is Tag tag)
            {
                var tagStr = tag.GetValue();
                if (TryGetHandler(tagStr, out var valHandler) && valHandler != null)
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object && valHandler is IDictionaryReadHandler dictHandler)
                        return ParseObjectElement(prop.Value, false, cache, dictHandler);
                    if (prop.Value.ValueKind == JsonValueKind.Array && valHandler is IListReadHandler listHandler)
                        return ParseArrayElement(prop.Value, false, cache, listHandler);
                    return valHandler.FromRepresentation(ParseElement(prop.Value, false, cache)!);
                }
                return Decode(tagStr, ParseElement(prop.Value, false, cache)!);
            }
            else
            {
                d = dr.Add(d, key, ParseElement(prop.Value, false, cache)!);
            }
        }

        return dr.Complete(d);
    }

    public override object? ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler? handler)
    {
        return ParseArrayElement(_root, asDictionaryKey, cache, handler);
    }

    private object? ParseArrayElement(JsonElement element, bool asDictionaryKey, ReadCache cache, IListReadHandler? handler)
    {
        int length = element.GetArrayLength();
        if (length == 0)
        {
            var lr2 = handler?.ListReader() ?? ListBuilder;
            return lr2.Complete(lr2.Init());
        }

        var enumerator = element.EnumerateArray();
        enumerator.MoveNext();
        var firstVal = ParseElement(enumerator.Current, false, cache);

        if (firstVal is string s && s == Constants.DirectoryAsList)
        {
            // Build a map from the rest of the array
            return ParseArrayAsDict(enumerator, cache, null);
        }

        if (firstVal is Tag firstTag)
        {
            if (enumerator.MoveNext())
            {
                var tagStr = firstTag.GetValue();
                if (TryGetHandler(tagStr, out var valHandler) && valHandler != null)
                {
                    var valElement = enumerator.Current;
                    object? val;
                    if (valElement.ValueKind == JsonValueKind.Object && valHandler is IDictionaryReadHandler dictHandler)
                        val = ParseObjectElement(valElement, false, cache, dictHandler);
                    else if (valElement.ValueKind == JsonValueKind.Array && valHandler is IDictionaryReadHandler dictHandler2)
                    {
                        // Map-as-array encoding (["^ ", ...])  fed to a dict handler
                        var parsed = ParseArrayElement(valElement, false, cache, null);
                        if (parsed is System.Collections.IDictionary parsedDict)
                        {
                            var dr = dictHandler2.DictionaryReader();
                            var result = dr.Init();
                            foreach (System.Collections.DictionaryEntry entry in parsedDict)
                                result = dr.Add(result, entry.Key, entry.Value!);
                            val = dr.Complete(result);
                        }
                        else
                        {
                            val = valHandler.FromRepresentation(parsed!);
                        }
                    }
                    else if (valElement.ValueKind == JsonValueKind.Array && valHandler is IListReadHandler listHandler)
                        val = ParseArrayElement(valElement, false, cache, listHandler);
                    else
                        val = valHandler.FromRepresentation(ParseElement(valElement, false, cache)!);
                    return val;
                }

                return Decode(tagStr, ParseElement(enumerator.Current, false, cache)!);
            }
        }

        // Normal list
        var lr = handler?.ListReader() ?? ListBuilder;
        var l = lr.Init();
        l = lr.Add(l, firstVal!);
        while (enumerator.MoveNext())
        {
            l = lr.Add(l, ParseElement(enumerator.Current, false, cache)!);
        }
        return lr.Complete(l);
    }

    private object? ParseArrayAsDict(JsonElement.ArrayEnumerator enumerator, ReadCache cache, IDictionaryReadHandler? handler)
    {
        var dr = handler?.DictionaryReader() ?? DictionaryBuilder;
        var d = dr.Init();

        while (enumerator.MoveNext())
        {
            var key = ParseElement(enumerator.Current, true, cache);
            if (key is Tag tag)
            {
                if (enumerator.MoveNext())
                {
                    var tagStr = tag.GetValue();
                    if (TryGetHandler(tagStr, out var valHandler) && valHandler != null)
                    {
                        var valElement = enumerator.Current;
                        object? val;
                        if (valElement.ValueKind == JsonValueKind.Object && valHandler is IDictionaryReadHandler dictHandler)
                            val = ParseObjectElement(valElement, false, cache, dictHandler);
                        else if (valElement.ValueKind == JsonValueKind.Array && valHandler is IListReadHandler listHandler)
                            val = ParseArrayElement(valElement, false, cache, listHandler);
                        else
                            val = valHandler.FromRepresentation(ParseElement(valElement, false, cache)!);

                        // Read past end marker
                        enumerator.MoveNext();
                        return val;
                    }
                    return Decode(tagStr, ParseElement(enumerator.Current, false, cache)!);
                }
            }
            else
            {
                if (enumerator.MoveNext())
                {
                    d = dr.Add(d, key!, ParseElement(enumerator.Current, false, cache)!);
                }
            }
        }

        return dr.Complete(d);
    }
}
