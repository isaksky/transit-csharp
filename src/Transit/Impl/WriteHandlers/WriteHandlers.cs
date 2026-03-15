namespace Transit.Impl.WriteHandlers;

internal sealed class NullWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "_";
    public override object Representation(object obj) => null!;
    public override string? StringRepresentation(object obj) => "";
}

internal sealed class BooleanWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "?";
    public override object Representation(object obj) => obj;
    public override string? StringRepresentation(object obj) => ((bool)obj) ? "t" : "f";
}

internal sealed class ToStringWriteHandler : AbstractWriteHandler
{
    private readonly string _tag;
    public ToStringWriteHandler(string tag) => _tag = tag;
    public override string Tag(object obj) => _tag;
    public override object Representation(object obj) => obj.ToString()!;
    public override string? StringRepresentation(object obj) => obj.ToString();
}

internal sealed class IntegerWriteHandler : AbstractWriteHandler
{
    private readonly string _tag;
    public IntegerWriteHandler(string tag) => _tag = tag;
    public override string Tag(object obj) => _tag;
    public override object Representation(object obj) => obj;
    public override string? StringRepresentation(object obj) => obj.ToString();
}

internal sealed class FloatWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj)
    {
        float f = (float)obj;
        return float.IsNaN(f) || float.IsInfinity(f) ? "z" : "d";
    }
    public override object Representation(object obj)
    {
        float f = (float)obj;
        if (float.IsNaN(f)) return "NaN";
        if (float.IsPositiveInfinity(f)) return "INF";
        if (float.IsNegativeInfinity(f)) return "-INF";
        return obj;
    }
    public override string? StringRepresentation(object obj) => Representation(obj).ToString();
}

internal sealed class DoubleWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj)
    {
        double d = (double)obj;
        return double.IsNaN(d) || double.IsInfinity(d) ? "z" : "d";
    }
    public override object Representation(object obj)
    {
        double d = (double)obj;
        if (double.IsNaN(d)) return "NaN";
        if (double.IsPositiveInfinity(d)) return "INF";
        if (double.IsNegativeInfinity(d)) return "-INF";
        return obj;
    }
    public override string? StringRepresentation(object obj) => Representation(obj).ToString();
}

internal sealed class BinaryWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "b";
    public override object Representation(object obj) => obj;
    public override string? StringRepresentation(object obj) => Convert.ToBase64String((byte[])obj);
}

internal sealed class GuidWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "u";
    public override object Representation(object obj) => obj;
    public override string? StringRepresentation(object obj) => ((Guid)obj).ToString();
}

internal sealed class DateTimeWriteHandler : AbstractWriteHandler
{
    private sealed class VerboseDateTimeWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj) => "t";
        public override object Representation(object obj) => AbstractParser.FormatDateTime((DateTime)obj);
        public override string? StringRepresentation(object obj) => Representation(obj).ToString();
    }

    private static readonly IWriteHandler VerboseHandler = new VerboseDateTimeWriteHandler();

    public override string Tag(object obj) => "m";
    public override object Representation(object obj) => Transit.Java.Convert.ToJavaTime((DateTime)obj);
    public override string? StringRepresentation(object obj) => Representation(obj).ToString();
    public override IWriteHandler? GetVerboseHandler() => VerboseHandler;
}

internal sealed class QuoteWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "'";
    public override object Representation(object obj) => ((Quote)obj).Value!;
}

internal sealed class TaggedValueWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => ((ITaggedValue)obj).Tag;
    public override object Representation(object obj) => ((ITaggedValue)obj).Representation;
}

internal sealed class RatioWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "ratio";
    public override object Representation(object obj)
    {
        var r = (IRatio)obj;
        return TransitFactory.TaggedValue("array", new List<object>
        {
            r.Numerator,
            r.Denominator,
        });
    }
}

internal sealed class LinkWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "link";
    public override object Representation(object obj)
    {
        var link = (ILink)obj;
        var d = new Dictionary<object, object>
        {
            { TransitFactory.Keyword("href"), link.Href.ToString() },
            { TransitFactory.Keyword("rel"), link.Rel },
        };
        if (link.Name != null) d[TransitFactory.Keyword("name")] = link.Name;
        if (link.Prompt != null) d[TransitFactory.Keyword("prompt")] = link.Prompt;
        if (link.Render != null) d[TransitFactory.Keyword("render")] = link.Render;
        return TransitFactory.TaggedValue("map", d);
    }
}

internal sealed class SetWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "set";
    public override object Representation(object obj)
    {
        var list = new List<object>();
        foreach (var item in (System.Collections.IEnumerable)obj)
            list.Add(item);
        return TransitFactory.TaggedValue("array", list);
    }
}

internal sealed class ListWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "array";
    public override object Representation(object obj) => obj;
}

internal sealed class EnumerableWriteHandler : AbstractWriteHandler
{
    public override string Tag(object obj) => "list";
    public override object Representation(object obj)
    {
        var list = new List<object>();
        foreach (var item in (System.Collections.IEnumerable)obj)
            list.Add(item);
        return TransitFactory.TaggedValue("array", list);
    }
}

internal sealed class DictionaryWriteHandler : AbstractWriteHandler, IAbstractEmitterAware
{
    private AbstractEmitter? _emitter;

    public void SetEmitter(AbstractEmitter emitter) => _emitter = emitter;

    private bool StringableKeys(System.Collections.IDictionary d)
    {
        foreach (var key in d.Keys)
        {
            var tag = _emitter!.GetTag(key);
            if (tag != null && tag.Length > 1)
                return false;
            if (tag == null && key is not string)
                return false;
        }
        return true;
    }

    public override string Tag(object obj) =>
        StringableKeys((System.Collections.IDictionary)obj) ? "map" : "cmap";

    public override object Representation(object obj)
    {
        var dict = (System.Collections.IDictionary)obj;
        if (StringableKeys(dict))
            return dict;

        var list = new List<object>();
        foreach (System.Collections.DictionaryEntry entry in dict)
        {
            list.Add(entry.Key);
            list.Add(entry.Value!);
        }
        return TransitFactory.TaggedValue("array", list);
    }
}
