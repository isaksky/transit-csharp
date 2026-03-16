using System.Collections;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transit;

namespace Transit.Tests;

[TestClass]
public class TransitConvertTests
{
    public enum SampleEnum
    {
        First,
        Second,
        Third
    }

    public class ChildPoco
    {
        public string Name { get; set; } = "";
        public int Age;
    }

    public class ParentPoco
    {
        public string Title { get; set; } = "";
        public ChildPoco Child { get; set; } = new();
        public SampleEnum Type { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public (int, string) TupleData { get; set; }
    }

    [TestMethod]
    public void TestRoundTripPoco()
    {
        var parent = new ParentPoco
        {
            Title = "Parent",
            Child = new ChildPoco { Name = "Child", Age = 10 },
            Type = SampleEnum.Second,
            Duration = TimeSpan.FromHours(1.5),
            Timestamp = DateTimeOffset.Now,
            TupleData = (42, "Answer")
        };

        var json = TransitConvert.SerializeObject(parent, TransitFactory.Format.Json);
        var deserialized = TransitConvert.DeserializeObject<ParentPoco>(json, TransitFactory.Format.Json);

        Assert.AreEqual(parent.Title, deserialized.Title);
        Assert.AreEqual(parent.Child.Name, deserialized.Child.Name);
        Assert.AreEqual(parent.Child.Age, deserialized.Child.Age);
        Assert.AreEqual(parent.Type, deserialized.Type);
        Assert.AreEqual(parent.Duration, deserialized.Duration);
        // Precision might be slightly different depending on string format, but TimeSpan.Parse should be exact for ticks
        Assert.AreEqual(parent.Timestamp.ToUnixTimeMilliseconds(), deserialized.Timestamp.ToUnixTimeMilliseconds());
        Assert.AreEqual(parent.TupleData.Item1, deserialized.TupleData.Item1);
        Assert.AreEqual(parent.TupleData.Item2, deserialized.TupleData.Item2);
    }

    [TestMethod]
    public void TestRoundTripPocoWithKeywords()
    {
        var settings = new TransitSerializerSettings { UseKeywordKeys = true };
        var child = new ChildPoco { Name = "KeywordChild", Age = 5 };

        var json = TransitConvert.SerializeObject(child, TransitFactory.Format.Json, settings);
        
        // Verify keywords are present in JSON (~:Name and ~:Age)
        Assert.IsTrue(json.Contains("~:Name"));
        Assert.IsTrue(json.Contains("~:Age"));

        var deserialized = TransitConvert.DeserializeObject<ChildPoco>(json, TransitFactory.Format.Json, settings);

        Assert.AreEqual(child.Name, deserialized.Name);
        Assert.AreEqual(child.Age, deserialized.Age);
    }

    [TestMethod]
    public void TestBuiltInTypesDirectly()
    {
        var ts = TimeSpan.FromMinutes(45);
        var dto = DateTimeOffset.Now;

        var tsJson = TransitConvert.SerializeObject(ts, TransitFactory.Format.Json);
        var dtoJson = TransitConvert.SerializeObject(dto, TransitFactory.Format.Json);

        Assert.AreEqual(ts, TransitConvert.DeserializeObject<TimeSpan>(tsJson, TransitFactory.Format.Json));
        Assert.AreEqual(dto.ToUnixTimeMilliseconds(), TransitConvert.DeserializeObject<DateTimeOffset>(dtoJson, TransitFactory.Format.Json).ToUnixTimeMilliseconds());
    }

    [TestMethod]
    public void TestEnumDirectly()
    {
        var val = SampleEnum.Third;
        var json = TransitConvert.SerializeObject(val, TransitFactory.Format.Json);
        Assert.AreEqual(val, TransitConvert.DeserializeObject<SampleEnum>(json, TransitFactory.Format.Json));
    }

    [TestMethod]
    public void TestCachingPerformance()
    {
        var poco = new ChildPoco { Name = "Test", Age = 1 };
        
        // Warm up
        TransitConvert.SerializeObject(poco, TransitFactory.Format.Json);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            TransitConvert.SerializeObject(poco, TransitFactory.Format.Json);
        }
        sw.Stop();
        
        // 1000 serializations should be very fast (usually sub-100ms on modern machines)
        Assert.IsTrue(sw.ElapsedMilliseconds < 500, $"Serialization too slow: {sw.ElapsedMilliseconds}ms");
    }
}
