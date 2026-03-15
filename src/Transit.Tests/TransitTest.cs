using System.Collections;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transit;
using Transit.Impl;
using Transit.Numerics;

namespace Transit.Tests;

[TestClass]
public class TransitTest
{
    #region Reading

    private IReader Reader(string s)
    {
        var input = new MemoryStream(Encoding.UTF8.GetBytes(s));
        return TransitFactory.Reader(TransitFactory.Format.Json, input);
    }

    [TestMethod]
    public void TestReadString()
    {
        Assert.AreEqual("foo", Reader("\"foo\"").Read<string>());
        Assert.AreEqual("~foo", Reader("\"~~foo\"").Read<string>());
        Assert.AreEqual("`foo", Reader("\"~`foo\"").Read<string>());
        Assert.AreEqual("foo", Reader("\"~#foo\"").Read<Tag>().GetValue());
        Assert.AreEqual("^foo", Reader("\"~^foo\"").Read<string>());
    }

    [TestMethod]
    public void TestReadBoolean()
    {
        Assert.IsTrue(Reader("\"~?t\"").Read<bool>());
        Assert.IsFalse(Reader("\"~?f\"").Read<bool>());

        var d = Reader("{\"~?t\":1,\"~?f\":2}").Read<IDictionary>();
        Assert.AreEqual(1L, d[true]);
        Assert.AreEqual(2L, d[false]);
    }

    [TestMethod]
    public void TestReadNull()
    {
        var v = Reader("\"~_\"").Read<object>();
        Assert.IsNull(v);
    }

    [TestMethod]
    public void TestReadKeyword()
    {
        var v = Reader("\"~:foo\"").Read<IKeyword>();
        Assert.AreEqual("foo", v.ToString());

        var r = Reader("[\"~:foo\",\"^" + (char)WriteCache.BaseCharIdx + "\",\"^" + (char)WriteCache.BaseCharIdx + "\"]");
        var v2 = r.Read<IList>();
        Assert.AreEqual("foo", v2[0]!.ToString());
        Assert.AreEqual("foo", v2[1]!.ToString());
        Assert.AreEqual("foo", v2[2]!.ToString());
    }

    [TestMethod]
    public void TestReadInteger()
    {
        var v = Reader("\"~i42\"").Read<long>();
        Assert.AreEqual(42L, v);
    }

    [TestMethod]
    public void TestReadBigInteger()
    {
        var expected = BigInteger.Parse("4256768765123454321897654321234567");
        var v = Reader("\"~n4256768765123454321897654321234567\"").Read<BigInteger>();
        Assert.AreEqual(expected, v);
    }

    [TestMethod]
    public void TestReadDouble()
    {
        Assert.AreEqual(42.5D, Reader("\"~d42.5\"").Read<double>());
    }

    [TestMethod]
    public void TestReadSpecialNumbers()
    {
        Assert.AreEqual(double.NaN, Reader("\"~zNaN\"").Read<double>());
        Assert.AreEqual(double.PositiveInfinity, Reader("\"~zINF\"").Read<double>());
        Assert.AreEqual(double.NegativeInfinity, Reader("\"~z-INF\"").Read<double>());
    }

    [TestMethod]
    public void TestReadBigRational()
    {
        Assert.AreEqual(new BigRational(12.345M), Reader("\"~f12.345\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(-12.345M), Reader("\"~f-12.345\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(0.1001M), Reader("\"~f0.1001\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(0.01M), Reader("\"~f0.01\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(0.1M), Reader("\"~f0.1\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(1M), Reader("\"~f1\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(10M), Reader("\"~f10\"").Read<BigRational>());
        Assert.AreEqual(new BigRational(420.0057M), Reader("\"~f420.0057\"").Read<BigRational>());
    }

    [TestMethod]
    public void TestReadDateTime()
    {
        var d = new DateTime(2014, 8, 9, 10, 6, 21, 497, DateTimeKind.Local);
        var expected = new DateTimeOffset(d).LocalDateTime;
        long javaTime = Transit.Java.Convert.ToJavaTime(d);

        string timeString = AbstractParser.FormatDateTime(d);
        Assert.AreEqual(expected, Reader("\"~t" + timeString + "\"").Read<DateTime>());

        Assert.AreEqual(expected, Reader("{\"~#m\": " + javaTime + "}").Read<DateTime>());

        timeString = new DateTimeOffset(d).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        Assert.AreEqual(expected, Reader("\"~t" + timeString + "\"").Read<DateTime>());

        timeString = new DateTimeOffset(d).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        Assert.AreEqual(expected.AddMilliseconds(-497D), Reader("\"~t" + timeString + "\"").Read<DateTime>());

        timeString = new DateTimeOffset(d).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff-00:00");
        Assert.AreEqual(expected, Reader("\"~t" + timeString + "\"").Read<DateTime>());
    }

    [TestMethod]
    public void TestReadGuid()
    {
        var guid = Guid.NewGuid();
        long hi64 = ((Transit.Java.Uuid)guid).MostSignificantBits;
        long lo64 = ((Transit.Java.Uuid)guid).LeastSignificantBits;

        Assert.AreEqual(guid, Reader("\"~u" + guid.ToString() + "\"").Read<Guid>());
        Assert.AreEqual(guid, Reader("{\"~#u\": [" + hi64 + ", " + lo64 + "]}").Read<Guid>());
    }

    [TestMethod]
    public void TestReadUri()
    {
        var expected = new Uri("http://www.foo.com");
        var v = Reader("\"~rhttp://www.foo.com\"").Read<Uri>();
        Assert.AreEqual(expected, v);
    }

    [TestMethod]
    public void TestReadSymbol()
    {
        var v = Reader("\"~$foo\"").Read<ISymbol>();
        Assert.AreEqual("foo", v.ToString());
    }

    [TestMethod]
    public void TestReadCharacter()
    {
        var v = Reader("\"~cf\"").Read<char>();
        Assert.AreEqual('f', v);
    }

    [TestMethod]
    public void TestReadBinary()
    {
        byte[] bytes = Encoding.ASCII.GetBytes("foobarbaz");
        string encoded = Convert.ToBase64String(bytes);
        byte[] decoded = Reader("\"~b" + encoded + "\"").Read<byte[]>();

        Assert.AreEqual(bytes.Length, decoded.Length);
        CollectionAssert.AreEqual(bytes, decoded);
    }

    [TestMethod]
    public void TestReadUnknown()
    {
        var result1 = Reader("\"~jfoo\"").Read<ITaggedValue>();
        Assert.AreEqual("j", result1.Tag);
        Assert.AreEqual("foo", result1.Representation);

        IList<object> l = new List<object> { 1L, 2L };
        var result = Reader("{\"~#point\":[1,2]}").Read<ITaggedValue>();
        Assert.AreEqual("point", result.Tag);
        var resultList = (IList<object>)result.Representation;
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual(1L, resultList[0]);
        Assert.AreEqual(2L, resultList[1]);
    }

    [TestMethod]
    public void TestReadList()
    {
        var l = Reader("[1, 2, 3]").Read<IList>();

        Assert.IsInstanceOfType<IList<object>>(l);
        Assert.AreEqual(3, l.Count);
        Assert.AreEqual(1L, l[0]);
        Assert.AreEqual(2L, l[1]);
        Assert.AreEqual(3L, l[2]);
    }

    [TestMethod]
    public void TestReadListWithNested()
    {
        var d = new DateTime(2014, 8, 10, 13, 34, 35);
        string t = AbstractParser.FormatDateTime(d);

        var l = Reader("[\"~:foo\", \"~t" + t + "\", \"~?t\"]").Read<IList>();

        Assert.AreEqual(3, l.Count);
        Assert.AreEqual("foo", l[0]!.ToString());
        Assert.AreEqual(d, (DateTime)l[1]!);
        Assert.IsTrue((bool)l[2]!);
    }

    [TestMethod]
    public void TestReadArrayWithNestedDoubles()
    {
        var l = Reader("[-3.14159, 3.14159, 4.0E11, 2.998E8, 6.626E-34]").Read<IList>();
        Assert.AreEqual(5, l.Count);
        Assert.AreEqual(-3.14159, (double)l[0]!, 0.00001);
        Assert.AreEqual(3.14159, (double)l[1]!, 0.00001);
        Assert.AreEqual(4.0E11, (double)l[2]!, 1.0);
        Assert.AreEqual(2.998E8, (double)l[3]!, 1.0);
        Assert.AreEqual(6.626E-34, (double)l[4]!, 1E-40);
    }

    [TestMethod]
    public void TestReadDictionary()
    {
        var m = Reader("{\"a\": 2, \"b\": 4}").Read<IDictionary>();

        Assert.AreEqual(2, m.Count);
        Assert.AreEqual(2L, m["a"]);
        Assert.AreEqual(4L, m["b"]);
    }

    [TestMethod]
    public void TestReadDictionaryWithNested()
    {
        var guid = Guid.NewGuid();

        var m = Reader("{\"a\": \"~:foo\", \"b\": \"~u" + guid + "\"}").Read<IDictionary>();

        Assert.AreEqual(2, m.Count);
        Assert.AreEqual("foo", m["a"]!.ToString());
        Assert.AreEqual(guid, m["b"]);
    }

    [TestMethod]
    public void TestReadSet()
    {
        var s = Reader("{\"~#set\": [1, 2, 3]}").Read<ISet<object>>();

        Assert.AreEqual(3, s.Count);
        Assert.IsTrue(s.Contains(1L));
        Assert.IsTrue(s.Contains(2L));
        Assert.IsTrue(s.Contains(3L));
    }

    [TestMethod]
    public void TestReadEnumerable()
    {
        var l = Reader("{\"~#list\": [1, 2, 3]}").Read<IEnumerable>();
        var lo = l.OfType<object>().ToList();

        Assert.AreEqual(3, lo.Count);
        Assert.AreEqual(1L, lo[0]);
        Assert.AreEqual(2L, lo[1]);
        Assert.AreEqual(3L, lo[2]);
    }

    [TestMethod]
    public void TestReadRatio()
    {
        var r = Reader("{\"~#ratio\": [\"~n1\",\"~n2\"]}").Read<IRatio>();

        Assert.AreEqual(BigInteger.One, r.Numerator);
        Assert.AreEqual(BigInteger.One + 1, r.Denominator);
        Assert.AreEqual(0.5d, r.GetValue(), 0.01);
    }

    [TestMethod]
    public void TestReadCDictionary()
    {
        var m = Reader("{\"~#cmap\": [{\"~#ratio\":[\"~n1\",\"~n2\"]},1,{\"~#list\":[1,2,3]},2]}").Read<IDictionary>();

        Assert.AreEqual(2, m.Count);

        foreach (DictionaryEntry e in m)
        {
            if ((long)e.Value! == 1L)
            {
                var r = (IRatio)e.Key;
                Assert.AreEqual(new BigInteger(1), r.Numerator);
                Assert.AreEqual(new BigInteger(2), r.Denominator);
            }
            else if ((long)e.Value! == 2L)
            {
                var l = ((IEnumerable<object>)e.Key).ToList();
                Assert.AreEqual(1L, l[0]);
                Assert.AreEqual(2L, l[1]);
                Assert.AreEqual(3L, l[2]);
            }
        }
    }

    [TestMethod]
    public void TestReadCDictionaryWithNullKey()
    {
        var m2 = Reader("[\"~#cmap\",[null,\"null as map key\",[\"1\",\"2\"],\"Array as key to force cmap\"]]").Read<IDictionary>();
        Assert.AreEqual(2, m2.Count);
        Assert.AreEqual("null as map key", m2[null!]);
        var key = new List<object> { "1", "2" };
        // Find entry with array key by iterating (since List doesn't implement structural equality for dictionary lookup)
        string? arrayKeyValue = null;
        foreach (DictionaryEntry e in m2)
        {
            if (e.Key is IList<object> l && l.Count == 2 && l[0].Equals("1") && l[1].Equals("2"))
            {
                arrayKeyValue = (string)e.Value!;
            }
        }
        Assert.AreEqual("Array as key to force cmap", arrayKeyValue);
        Assert.IsTrue(m2.Contains(null!));
    }

    [TestMethod]
    public void TestReadSetTagAsString()
    {
        var o = Reader("{\"~~#set\": [1, 2, 3]}").Read<object>();
        Assert.IsFalse(o is ISet<object>);
        Assert.IsTrue(o is IDictionary);
    }

    [TestMethod]
    public void TestReadMany()
    {
        Assert.IsTrue(Reader("true").Read<bool>());
        Assert.IsNull(Reader("null").Read<object>());
        Assert.IsFalse(Reader("false").Read<bool>());
        Assert.AreEqual("foo", Reader("\"foo\"").Read<string>());
        Assert.AreEqual(42.2, Reader("42.2").Read<double>());
        Assert.AreEqual(42L, Reader("42").Read<long>());
    }

    [TestMethod]
    public void TestReadCache()
    {
        var rc = new ReadCache();
        Assert.AreEqual("~:foo", rc.CacheRead("~:foo", false));
        Assert.AreEqual("~:foo", rc.CacheRead("^" + (char)WriteCache.BaseCharIdx, false));
        Assert.AreEqual("~$bar", rc.CacheRead("~$bar", false));
        Assert.AreEqual("~$bar", rc.CacheRead("^" + (char)(WriteCache.BaseCharIdx + 1), false));
        Assert.AreEqual("~#baz", rc.CacheRead("~#baz", false));
        Assert.AreEqual("~#baz", rc.CacheRead("^" + (char)(WriteCache.BaseCharIdx + 2), false));
        Assert.AreEqual("foobar", rc.CacheRead("foobar", false));
        Assert.AreEqual("foobar", rc.CacheRead("foobar", false));
        Assert.AreEqual("foobar", rc.CacheRead("foobar", true));
        Assert.AreEqual("foobar", rc.CacheRead("^" + (char)(WriteCache.BaseCharIdx + 3), true));
        Assert.AreEqual("abc", rc.CacheRead("abc", false));
        Assert.AreEqual("abc", rc.CacheRead("abc", false));
        Assert.AreEqual("abc", rc.CacheRead("abc", true));
        Assert.AreEqual("abc", rc.CacheRead("abc", true));
    }

    [TestMethod]
    public void TestReadIdentity()
    {
        // System.Text.Json doesn't accept \' escape - use the valid encoding
        var v = Reader("\"~'42\"").Read<string>();
        Assert.AreEqual("42", v);
    }

    [TestMethod]
    public void TestReadWithNoCustomHandlers()
    {
        var input = new MemoryStream(Encoding.UTF8.GetBytes("\"foo\""));
        var reader = TransitFactory.Reader(TransitFactory.Format.Json, input, null, null);
        Assert.AreEqual("foo", reader.Read<string>());
    }

    [TestMethod]
    public void TestReadLink()
    {
        var r = Reader("[\"~#link\" , {\"href\": \"~rhttp://www.Beerendonk.nl\", \"rel\": \"a-rel\", \"name\": \"a-name\", \"prompt\": \"a-prompt\", \"render\": \"link or image\"}]");
        var v = r.Read<ILink>();
        Assert.AreEqual(new Uri("http://www.Beerendonk.nl"), v.Href);
        Assert.AreEqual("a-rel", v.Rel);
        Assert.AreEqual("a-name", v.Name);
        Assert.AreEqual("a-prompt", v.Prompt);
        Assert.AreEqual("link or image", v.Render);
    }

    [TestMethod]
    public void TestCustomReadHandler()
    {
        var customHandlers = new Dictionary<string, IReadHandler>
        {
            ["point"] = new PointReadHandler()
        };
        var input = new MemoryStream(Encoding.UTF8.GetBytes("[\"~#point\",[37,42]]"));
        var reader = TransitFactory.Reader(TransitFactory.Format.Json, input, customHandlers, null);
        var result = reader.Read<Point>();
        Assert.AreEqual(new Point(37, 42), result);
    }

    [TestMethod]
    public void TestCustomDefaultReadHandler()
    {
        var defaultHandler = new CatchAllDefaultReadHandler();
        var input = new MemoryStream(Encoding.UTF8.GetBytes("[\"~#unknown\",[37,42]]"));
        var reader = TransitFactory.Reader(TransitFactory.Format.Json, input, null, defaultHandler);
        var result = reader.Read<string>();
        Assert.AreEqual("unknown: [37, 42]", result);
    }

    private record Point(int X, int Y);

    private class PointReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            var coords = (IList<object>)representation;
            int x = System.Convert.ToInt32(coords[0]);
            int y = System.Convert.ToInt32(coords[1]);
            return new Point(x, y);
        }
    }

    private class CatchAllDefaultReadHandler : IDefaultReadHandler<object>
    {
        public object FromRepresentation(string tag, object representation)
        {
            // Format collections like Java's ArrayList.toString() for consistent output
            if (representation is IList<object> list)
                return $"{tag}: [{string.Join(", ", list)}]";
            return $"{tag}: {representation}";
        }
    }

    #endregion

    #region Writing

    private string Write(object? obj, TransitFactory.Format format, IDictionary<Type, IWriteHandler>? customHandlers)
    {
        using var output = new MemoryStream();
        var w = TransitFactory.Writer<object>(format, output, customHandlers);
        w.Write(obj!);

        output.Position = 0;
        var sr = new StreamReader(output);
        return sr.ReadToEnd();
    }

    private string WriteJsonVerbose(object? obj) => Write(obj, TransitFactory.Format.JsonVerbose, null);
    private string WriteJsonVerbose(object? obj, IDictionary<Type, IWriteHandler> customHandlers)
        => Write(obj, TransitFactory.Format.JsonVerbose, customHandlers);

    private string WriteJson(object? obj) => Write(obj, TransitFactory.Format.Json, null);
    private string WriteJson(object? obj, IDictionary<Type, IWriteHandler> customHandlers)
        => Write(obj, TransitFactory.Format.Json, customHandlers);

    private static string Scalar(string value) => "[\"~#'\"," + value + "]";
    private static string ScalarVerbose(string value) => "{\"~#'\":" + value + "}";

    [TestMethod]
    public void TestWriteNull()
    {
        Assert.AreEqual(ScalarVerbose("null"), WriteJsonVerbose(null));
        Assert.AreEqual(Scalar("null"), WriteJson(null));
    }

    [TestMethod]
    public void TestWriteKeyword()
    {
        Assert.AreEqual(ScalarVerbose("\"~:foo\""), WriteJsonVerbose(TransitFactory.Keyword("foo")));
        Assert.AreEqual(Scalar("\"~:foo\""), WriteJson(TransitFactory.Keyword("foo")));

        IList l = new IKeyword[]
        {
            TransitFactory.Keyword("foo"),
            TransitFactory.Keyword("foo"),
            TransitFactory.Keyword("foo")
        };
        Assert.AreEqual("[\"~:foo\",\"~:foo\",\"~:foo\"]", WriteJsonVerbose(l));
        Assert.AreEqual("[\"~:foo\",\"^0\",\"^0\"]", WriteJson(l));
    }

    [TestMethod]
    public void TestWriteObjectJsonThrows()
    {
        Assert.ThrowsException<NotSupportedException>(() => WriteJson(new object()));
    }

    [TestMethod]
    public void TestWriteObjectJsonVerboseThrows()
    {
        Assert.ThrowsException<NotSupportedException>(() => WriteJsonVerbose(new object()));
    }

    [TestMethod]
    public void TestWriteString()
    {
        Assert.AreEqual(ScalarVerbose("\"foo\""), WriteJsonVerbose("foo"));
        Assert.AreEqual(Scalar("\"foo\""), WriteJson("foo"));
        Assert.AreEqual(ScalarVerbose("\"~~foo\""), WriteJsonVerbose("~foo"));
        Assert.AreEqual(Scalar("\"~~foo\""), WriteJson("~foo"));
    }

    [TestMethod]
    public void TestWriteBoolean()
    {
        Assert.AreEqual(ScalarVerbose("true"), WriteJsonVerbose(true));
        Assert.AreEqual(Scalar("true"), WriteJson(true));
        Assert.AreEqual(Scalar("false"), WriteJson(false));

        var d = new Dictionary<bool, int> { [true] = 1 };
        Assert.AreEqual("{\"~?t\":1}", WriteJsonVerbose(d));
        Assert.AreEqual("[\"^ \",\"~?t\",1]", WriteJson(d));

        var d2 = new Dictionary<bool, int> { [false] = 1 };
        Assert.AreEqual("{\"~?f\":1}", WriteJsonVerbose(d2));
        Assert.AreEqual("[\"^ \",\"~?f\",1]", WriteJson(d2));
    }

    [TestMethod]
    public void TestWriteInteger()
    {
        Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose(42));
        Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose(42L));
        Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose((byte)42));
        Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose((short)42));
        Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose((int)42));
        Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose(42L));
        Assert.AreEqual(ScalarVerbose("\"~n42\""), WriteJsonVerbose(BigInteger.Parse("42")));
        Assert.AreEqual(ScalarVerbose("\"~n4256768765123454321897654321234567\""),
            WriteJsonVerbose(BigInteger.Parse("4256768765123454321897654321234567")));
    }

    [TestMethod]
    public void TestWriteIntegerAtJsonBoundaries()
    {
        // 2^53 - 1 is the max safe JSON integer — should be written as a bare number
        Assert.AreEqual(ScalarVerbose("9007199254740991"), WriteJsonVerbose((long)Math.Pow(2, 53) - 1));
        // 2^53 exceeds safe range — should be written as ~i string
        Assert.AreEqual(ScalarVerbose("\"~i9007199254740992\""), WriteJsonVerbose((long)Math.Pow(2, 53)));

        // Negative boundary
        Assert.AreEqual(ScalarVerbose("-9007199254740991"), WriteJsonVerbose(1 - (long)Math.Pow(2, 53)));
        Assert.AreEqual(ScalarVerbose("\"~i-9007199254740992\""), WriteJsonVerbose(0 - (long)Math.Pow(2, 53)));
    }

    [TestMethod]
    public void TestWriteFloatDouble()
    {
        Assert.AreEqual(ScalarVerbose("42.5"), WriteJsonVerbose(42.5));
        Assert.AreEqual(ScalarVerbose("42.5"), WriteJsonVerbose(42.5F));
        Assert.AreEqual(ScalarVerbose("42.5"), WriteJsonVerbose(42.5D));
    }

    [TestMethod]
    public void TestSpecialNumbers()
    {
        Assert.AreEqual(Scalar("\"~zNaN\""), WriteJson(double.NaN));
        Assert.AreEqual(Scalar("\"~zINF\""), WriteJson(double.PositiveInfinity));
        Assert.AreEqual(Scalar("\"~z-INF\""), WriteJson(double.NegativeInfinity));

        Assert.AreEqual(Scalar("\"~zNaN\""), WriteJson(float.NaN));
        Assert.AreEqual(Scalar("\"~zINF\""), WriteJson(float.PositiveInfinity));
        Assert.AreEqual(Scalar("\"~z-INF\""), WriteJson(float.NegativeInfinity));

        Assert.AreEqual(ScalarVerbose("\"~zNaN\""), WriteJsonVerbose(double.NaN));
        Assert.AreEqual(ScalarVerbose("\"~zINF\""), WriteJsonVerbose(double.PositiveInfinity));
        Assert.AreEqual(ScalarVerbose("\"~z-INF\""), WriteJsonVerbose(double.NegativeInfinity));

        Assert.AreEqual(ScalarVerbose("\"~zNaN\""), WriteJsonVerbose(float.NaN));
        Assert.AreEqual(ScalarVerbose("\"~zINF\""), WriteJsonVerbose(float.PositiveInfinity));
        Assert.AreEqual(ScalarVerbose("\"~z-INF\""), WriteJsonVerbose(float.NegativeInfinity));
    }

    [TestMethod]
    public void TestWriteDateTime()
    {
        var d = DateTime.Now;
        string dateString = AbstractParser.FormatDateTime(d);
        long dateLong = Transit.Java.Convert.ToJavaTime(d);
        Assert.AreEqual(ScalarVerbose("\"~t" + dateString + "\""), WriteJsonVerbose(d));
        Assert.AreEqual(Scalar("\"~m" + dateLong + "\""), WriteJson(d));
    }

    [TestMethod]
    public void TestWriteUUID()
    {
        var guid = Guid.NewGuid();
        Assert.AreEqual(ScalarVerbose("\"~u" + guid.ToString() + "\""), WriteJsonVerbose(guid));
    }

    [TestMethod]
    public void TestWriteURI()
    {
        var uri = new Uri("http://www.foo.com/");
        Assert.AreEqual(ScalarVerbose("\"~rhttp://www.foo.com/\""), WriteJsonVerbose(uri));
    }

    [TestMethod]
    public void TestWriteBinary()
    {
        byte[] bytes = Encoding.ASCII.GetBytes("foobarbaz");
        string encoded = Convert.ToBase64String(bytes);
        Assert.AreEqual(ScalarVerbose("\"~b" + encoded + "\""), WriteJsonVerbose(bytes));
    }

    [TestMethod]
    public void TestWriteSymbol()
    {
        Assert.AreEqual(ScalarVerbose("\"~$foo\""), WriteJsonVerbose(TransitFactory.Symbol("foo")));
    }

    [TestMethod]
    public void TestWriteList()
    {
        IList<int> l = new List<int> { 1, 2, 3 };
        Assert.AreEqual("[1,2,3]", WriteJsonVerbose(l));
        Assert.AreEqual("[1,2,3]", WriteJson(l));
    }

    [TestMethod]
    public void TestWritePrimitiveArrays()
    {
        int[] ints = { 1, 2 };
        Assert.AreEqual("[1,2]", WriteJsonVerbose(ints));

        long[] longs = { 1L, 2L };
        Assert.AreEqual("[1,2]", WriteJsonVerbose(longs));

        float[] floats = { 1.5f, 2.78f };
        Assert.AreEqual("[1.5,2.78]", WriteJsonVerbose(floats));

        bool[] bools = { true, false };
        Assert.AreEqual("[true,false]", WriteJsonVerbose(bools));

        double[] doubles = { 1.654d, 2.8765d };
        Assert.AreEqual("[1.654,2.8765]", WriteJsonVerbose(doubles));

        short[] shorts = { 1, 2 };
        Assert.AreEqual("[1,2]", WriteJsonVerbose(shorts));

        char[] chars = { '5', '/' };
        Assert.AreEqual("[\"~c5\",\"~c/\"]", WriteJsonVerbose(chars));
    }

    [TestMethod]
    public void TestWriteDictionary()
    {
        IDictionary<string, int> d = new Dictionary<string, int> { {"foo", 1}, {"bar", 2} };
        Assert.AreEqual("{\"foo\":1,\"bar\":2}", WriteJsonVerbose(d));
        Assert.AreEqual("[\"^ \",\"foo\",1,\"bar\",2]", WriteJson(d));
    }

    [TestMethod]
    public void TestWriteEmptyDictionary()
    {
        IDictionary<object, object> d = new Dictionary<object, object>();
        Assert.AreEqual("{}", WriteJsonVerbose(d));
        Assert.AreEqual("[\"^ \"]", WriteJson(d));
    }

    [TestMethod]
    public void TestWriteSet()
    {
        ISet<string> s = new HashSet<string> { "foo", "bar" };
        Assert.AreEqual("{\"~#set\":[\"foo\",\"bar\"]}", WriteJsonVerbose(s));
        Assert.AreEqual("[\"~#set\",[\"foo\",\"bar\"]]", WriteJson(s));
    }

    [TestMethod]
    public void TestWriteEmptySet()
    {
        ISet<object> s = new HashSet<object>();
        Assert.AreEqual("{\"~#set\":[]}", WriteJsonVerbose(s));
        Assert.AreEqual("[\"~#set\",[]]", WriteJson(s));
    }

    [TestMethod]
    public void TestWriteEnumerable()
    {
        ICollection<string> c = new LinkedList<string>();
        c.Add("foo");
        c.Add("bar");
        IEnumerable<string> e = c;
        Assert.AreEqual("{\"~#list\":[\"foo\",\"bar\"]}", WriteJsonVerbose(e));
        Assert.AreEqual("[\"~#list\",[\"foo\",\"bar\"]]", WriteJson(e));
    }

    [TestMethod]
    public void TestWriteEmptyEnumerable()
    {
        IEnumerable<string> c = new LinkedList<string>();
        Assert.AreEqual("{\"~#list\":[]}", WriteJsonVerbose(c));
        Assert.AreEqual("[\"~#list\",[]]", WriteJson(c));
    }

    [TestMethod]
    public void TestWriteCharacter()
    {
        Assert.AreEqual(ScalarVerbose("\"~cf\""), WriteJsonVerbose('f'));
    }

    [TestMethod]
    public void TestWriteRatio()
    {
        IRatio r = new Ratio(BigInteger.One, new BigInteger(2));
        Assert.AreEqual("{\"~#ratio\":[\"~n1\",\"~n2\"]}", WriteJsonVerbose(r));
        Assert.AreEqual("[\"~#ratio\",[\"~n1\",\"~n2\"]]", WriteJson(r));
    }

    [TestMethod]
    public void TestWriteCDictionary()
    {
        IRatio r = new Ratio(BigInteger.One, new BigInteger(2));
        IDictionary<object, object> d = new Dictionary<object, object> { [r] = 1 };
        Assert.AreEqual("{\"~#cmap\":[{\"~#ratio\":[\"~n1\",\"~n2\"]},1]}", WriteJsonVerbose(d));
        Assert.AreEqual("[\"~#cmap\",[[\"~#ratio\",[\"~n1\",\"~n2\"]],1]]", WriteJson(d));
    }

    [TestMethod]
    public void TestWriteCDictionaryWithNullKey()
    {
        var d = new Transit.Impl.NullKeyDictionary();
        d[null] = "null as map key";
        d[new List<object> { "1", "2" }] = "Array as key to force cmap";

        // Verify it writes as cmap (since null and array can't be string keys)
        string json = WriteJson(d);
        Assert.IsTrue(json.Contains("\"~#cmap\""), "Should encode as cmap");

        // Roundtrip: read it back and verify entries
        var result = Reader(json).Read<IDictionary>();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("null as map key", result[null!]);
    }

    [TestMethod]
    public void TestWriteCache()
    {
        var wc = new WriteCache(true);
        Assert.AreEqual("~:foo", wc.CacheWrite("~:foo", false));
        Assert.AreEqual("^" + (char)WriteCache.BaseCharIdx, wc.CacheWrite("~:foo", false));
        Assert.AreEqual("~$bar", wc.CacheWrite("~$bar", false));
        Assert.AreEqual("^" + (char)(WriteCache.BaseCharIdx + 1), wc.CacheWrite("~$bar", false));
        Assert.AreEqual("~#baz", wc.CacheWrite("~#baz", false));
        Assert.AreEqual("^" + (char)(WriteCache.BaseCharIdx + 2), wc.CacheWrite("~#baz", false));
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", true));
        Assert.AreEqual("^" + (char)(WriteCache.BaseCharIdx + 3), wc.CacheWrite("foobar", true));
        Assert.AreEqual("abc", wc.CacheWrite("abc", false));
        Assert.AreEqual("abc", wc.CacheWrite("abc", false));
        Assert.AreEqual("abc", wc.CacheWrite("abc", true));
        Assert.AreEqual("abc", wc.CacheWrite("abc", true));
    }

    [TestMethod]
    public void TestWriteCacheDisabled()
    {
        var wc = new WriteCache(false);
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", true));
        Assert.AreEqual("foobar", wc.CacheWrite("foobar", true));
    }

    [TestMethod]
    public void TestWriteUnknown()
    {
        var l = new List<object> { "`jfoo" };
        Assert.AreEqual("[\"~`jfoo\"]", WriteJsonVerbose(l));
        Assert.AreEqual(ScalarVerbose("\"~`jfoo\""), WriteJsonVerbose("`jfoo"));

        var l2 = new List<object> { 1L, 2L };
        Assert.AreEqual("{\"~#point\":[1,2]}", WriteJsonVerbose(TransitFactory.TaggedValue("point", l2)));
    }

    [TestMethod]
    public void TestRoundTrip()
    {
        object inObject = true;

        string s;
        using (var output = new MemoryStream())
        {
            var w = TransitFactory.Writer<object>(TransitFactory.Format.JsonVerbose, output);
            w.Write(inObject);
            output.Position = 0;
            s = new StreamReader(output).ReadToEnd();
        }

        byte[] buffer = Encoding.ASCII.GetBytes(s);
        using var input = new MemoryStream(buffer);
        var reader = TransitFactory.Reader(TransitFactory.Format.Json, input);
        var outObject = reader.Read<object>();

        Assert.AreEqual(inObject, outObject);
    }

    [TestMethod]
    public void TestWriteHandlerCache()
    {
        var customHandlers = new Dictionary<Type, IWriteHandler>
        {
            [typeof(IList<object>)] = new TestListWriteHandler()
        };

        // Creating multiple writers with the same handler map should not throw
        for (int i = 0; i < 2; i++)
        {
            using var output = new MemoryStream();
            var w = TransitFactory.Writer<object>(TransitFactory.Format.Json, output, customHandlers);
        }
    }

    [TestMethod]
    public void TestCustomWriteHandler()
    {
        var customHandlers = new Dictionary<Type, IWriteHandler>
        {
            [typeof(Point)] = new PointWriteHandler()
        };
        Assert.AreEqual("[\"~#point\",[37,42]]", WriteJson(new Point(37, 42), customHandlers));
    }

    [TestMethod]
    public void TestWriteUnknownType()
    {
        // Writing an unregistered type should throw NotSupportedException
        Assert.ThrowsException<NotSupportedException>(() => WriteJson(new Point(1, 2)));
    }

    private class TestListWriteHandler : IWriteHandler
    {
        public string Tag(object obj) => obj is List<object> ? "array" : "list";
        public object Representation(object obj)
        {
            if (obj is LinkedList<object>)
                return TransitFactory.TaggedValue("array", obj);
            return obj;
        }
        public string? StringRepresentation(object obj) => null;
        public IWriteHandler? GetVerboseHandler() => null;
    }

    private class PointWriteHandler : IWriteHandler
    {
        public string Tag(object obj) => "point";
        public object Representation(object obj)
        {
            var p = (Point)obj;
            return new List<object> { p.X, p.Y };
        }
        public string? StringRepresentation(object obj) => Representation(obj).ToString()!;
        public IWriteHandler? GetVerboseHandler() => this;
    }

    #endregion

    #region JSON Machine Mode

    [TestMethod]
    public void TestMachineReadMap()
    {
        var m = Reader("[\"^ \",\"foo\",1,\"bar\",2]").Read<IDictionary>();
        Assert.IsTrue(m.Contains("foo"));
        Assert.IsTrue(m.Contains("bar"));
        Assert.AreEqual(1L, m["foo"]);
        Assert.AreEqual(2L, m["bar"]);
    }

    [TestMethod]
    public void TestMachineReadMapWithNested()
    {
        var m = Reader("[\"^ \",\"foo\",1,\"bar\",[\"^ \",\"baz\",3]]").Read<IDictionary>();
        Assert.IsTrue(m["bar"] is IDictionary);
        Assert.AreEqual(3L, ((IDictionary)m["bar"]!)["baz"]);
    }

    [TestMethod]
    public void TestMachineWriteMap()
    {
        var m = new Dictionary<string, object> { ["foo"] = 1 };
        Assert.AreEqual("[\"^ \",\"foo\",1]", WriteJson(m));

        // Tighter test with preserved order
        var m2 = new SortedDictionary<string, object> { ["bar"] = 2, ["foo"] = 1 };
        Assert.AreEqual("[\"^ \",\"bar\",2,\"foo\",1]", WriteJson(m2));
    }

    [TestMethod]
    public void TestMachineWriteEmptyMap()
    {
        var m = new Dictionary<string, object>();
        Assert.AreEqual("[\"^ \"]", WriteJson(m));
    }

    [TestMethod]
    public void TestMachineWritingArrayMarkerDirectly()
    {
        // If "^ " appears as an element, it must be escaped to "~^ " so it's not
        // misinterpreted as a map marker when read back.
        var l1 = new List<object> { "^ " };
        var s = WriteJson(l1);
        Assert.AreEqual("[\"~^ \"]", s);

        // Round-trip: read it back and verify
        var l2 = Reader(s).Read<IList<object>>();
        Assert.AreEqual(1, l2.Count);
        Assert.AreEqual("^ ", l2[0]);
    }

    [TestMethod]
    public void TestMachineReadTime()
    {
        // Machine timestamps: ~m prefix
        var human = Reader("[\"~t1776-07-04T12:00:00.000Z\",\"~t1970-01-01T00:00:00.000Z\",\"~t2000-01-01T12:00:00.000Z\",\"~t2014-04-07T22:17:17.000Z\"]")
            .Read<IList<object>>();
        var machine = Reader("[\"~m-6106017600000\",\"~m0\",\"~m946728000000\",\"~m1396909037000\"]")
            .Read<IList<object>>();

        Assert.AreEqual(4, human.Count);
        Assert.AreEqual(4, machine.Count);

        for (int i = 0; i < human.Count; i++)
        {
            var dh = (DateTime)human[i];
            var dm = (DateTime)machine[i];
            Assert.AreEqual(dh, dm, $"Mismatch at index {i}");
        }
    }

    [TestMethod]
    public void TestMachineRoundTrip()
    {
        object inObject = true;
        string s;
        using (var output = new MemoryStream())
        {
            var w = TransitFactory.Writer<object>(TransitFactory.Format.Json, output);
            w.Write(inObject);
            output.Position = 0;
            s = new StreamReader(output).ReadToEnd();
        }

        using var input = new MemoryStream(Encoding.UTF8.GetBytes(s));
        var reader = TransitFactory.Reader(TransitFactory.Format.Json, input);
        var outObject = reader.Read<object>();
        Assert.AreEqual(inObject, outObject);
    }

    #endregion

    #region Type Tests

    [TestMethod]
    public void TestUseIKeywordAsDictionaryKey()
    {
        IDictionary<object, object> d = new Dictionary<object, object>();
        d.Add(TransitFactory.Keyword("foo"), 1);
        d.Add("foo", 2);
        d.Add(TransitFactory.Keyword("bar"), 3);
        d.Add("bar", 4);

        Assert.AreEqual(1, d[TransitFactory.Keyword("foo")]);
        Assert.AreEqual(2, d["foo"]);
        Assert.AreEqual(3, d[TransitFactory.Keyword("bar")]);
        Assert.AreEqual(4, d["bar"]);
    }

    [TestMethod]
    public void TestUseISymbolAsDictionaryKey()
    {
        IDictionary<object, object> d = new Dictionary<object, object>();
        d.Add(TransitFactory.Symbol("foo"), 1);
        d.Add("foo", 2);
        d.Add(TransitFactory.Symbol("bar"), 3);
        d.Add("bar", 4);

        Assert.AreEqual(1, d[TransitFactory.Symbol("foo")]);
        Assert.AreEqual(2, d["foo"]);
        Assert.AreEqual(3, d[TransitFactory.Symbol("bar")]);
        Assert.AreEqual(4, d["bar"]);
    }

    [TestMethod]
    public void TestKeywordEquality()
    {
        var k1 = TransitFactory.Keyword("foo");
        var k2 = TransitFactory.Keyword("!foo"[1..]);
        var k3 = TransitFactory.Keyword("bar");

        Assert.AreEqual(k1, k2);
        Assert.AreEqual(k2, k1);
        Assert.AreNotEqual(k1, k3);
        Assert.AreNotEqual(k3, k1);
    }

    [TestMethod]
    public void TestKeywordHashCode()
    {
        var k1 = TransitFactory.Keyword("foo");
        var k2 = TransitFactory.Keyword("!foo"[1..]);
        var k3 = TransitFactory.Keyword("bar");

        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Assert.AreNotEqual(k3.GetHashCode(), k1.GetHashCode());
    }

    [TestMethod]
    public void TestKeywordComparator()
    {
        var l = new List<IKeyword>
        {
            TransitFactory.Keyword("bbb"),
            TransitFactory.Keyword("ccc"),
            TransitFactory.Keyword("abc"),
            TransitFactory.Keyword("dab"),
        };

        l.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.Ordinal));

        Assert.AreEqual("abc", l[0].ToString());
        Assert.AreEqual("bbb", l[1].ToString());
        Assert.AreEqual("ccc", l[2].ToString());
        Assert.AreEqual("dab", l[3].ToString());
    }

    [TestMethod]
    public void TestSymbolEquality()
    {
        var s1 = TransitFactory.Symbol("foo");
        var s2 = TransitFactory.Symbol("!foo"[1..]);
        var s3 = TransitFactory.Symbol("bar");

        Assert.AreEqual(s1, s2);
        Assert.AreEqual(s2, s1);
        Assert.AreNotEqual(s1, s3);
        Assert.AreNotEqual(s3, s1);
    }

    [TestMethod]
    public void TestSymbolHashCode()
    {
        var s1 = TransitFactory.Symbol("foo");
        var s2 = TransitFactory.Symbol("!foo"[1..]);
        var s3 = TransitFactory.Symbol("bar");

        Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
        Assert.AreNotEqual(s3.GetHashCode(), s1.GetHashCode());
    }

    [TestMethod]
    public void TestSymbolComparator()
    {
        var l = new List<ISymbol>
        {
            TransitFactory.Symbol("bbb"),
            TransitFactory.Symbol("ccc"),
            TransitFactory.Symbol("abc"),
            TransitFactory.Symbol("dab"),
        };

        l.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.Ordinal));

        Assert.AreEqual("abc", l[0].ToString());
        Assert.AreEqual("bbb", l[1].ToString());
        Assert.AreEqual("ccc", l[2].ToString());
        Assert.AreEqual("dab", l[3].ToString());
    }

    [TestMethod]
    public void TestDictionaryWithEscapedKey()
    {
        var d1 = new Dictionary<object, object> { { "~Gfoo", 20L } };
        string str = WriteJson(d1);

        var d2 = Reader(str).Read<IDictionary>();
        Assert.IsTrue(d2.Contains("~Gfoo"));
        Assert.AreEqual(20L, d2["~Gfoo"]);
    }

    [TestMethod]
    public void TestLink()
    {
        var l1 = TransitFactory.Link("http://google.com/", "search", "name", "a-prompt", "link");
        string str = WriteJson(l1);
        var l2 = Reader(str).Read<ILink>();
        Assert.AreEqual("http://google.com/", l2.Href.AbsoluteUri);
        Assert.AreEqual("search", l2.Rel);
        Assert.AreEqual("name", l2.Name);
        Assert.AreEqual("link", l2.Render);
        Assert.AreEqual("a-prompt", l2.Prompt);
    }

    [TestMethod]
    public void TestEmptySet()
    {
        string str = WriteJson(new HashSet<object>());
        Assert.IsInstanceOfType<ISet<object>>(Reader(str).Read<ISet<object>>());
    }

    #endregion
}
