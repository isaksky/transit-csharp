using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transit;
using CsCheck;
using FluentAssertions;

namespace Transit.Net.Tests
{
    [TestClass]
    public class TransitPropertyTests
    {
        private static object? Normalize(object? obj)
        {
            if (obj is null) return null;
            if (obj is int val) return (long)val; // Transit uniformly deserializes all standard integers to Int64
            if (obj is System.Collections.IDictionary dict)
            {
                var d = new Dictionary<object, object?>();
                foreach (System.Collections.DictionaryEntry entry in dict)
                {
                    if (entry.Key != null)
                        d[Normalize(entry.Key)!] = Normalize(entry.Value);
                }
                return d;
            }
            if (obj is System.Collections.IList list && !(obj is byte[]))
            {
                var arr = new object?[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    arr[i] = Normalize(list[i]);
                }
                return arr;
            }
            return obj;
        }

        [TestMethod]
        public void Transit_Json_Roundtrips_Correctly()
        {
            Generators.AnyGen.Sample(value => 
            {
                var serialized = TransitConvert.SerializeObject(value, TransitFactory.Format.Json);
                var deserialized = TransitConvert.DeserializeObject<object>(serialized, TransitFactory.Format.Json);
                var normDeser = Normalize(deserialized);
                var normValue = Normalize(value);
                
                normDeser.Should().BeEquivalentTo(normValue, options => options
                    .RespectingRuntimeTypes()
                    .Using<DateTime>(ctx => ctx.Subject.ToUniversalTime().Should().BeCloseTo(ctx.Expectation.ToUniversalTime(), TimeSpan.FromMilliseconds(1)))
                    .WhenTypeIs<DateTime>());
            }, iter: 1000);
        }

        [TestMethod]
        public void Transit_JsonVerbose_Roundtrips_Correctly()
        {
            Generators.AnyGen.Sample(value => 
            {
                var serialized = TransitConvert.SerializeObject(value, TransitFactory.Format.JsonVerbose);
                var deserialized = TransitConvert.DeserializeObject<object>(serialized, TransitFactory.Format.JsonVerbose);
                var normDeser = Normalize(deserialized);
                var normValue = Normalize(value);
                
                normDeser.Should().BeEquivalentTo(normValue, options => options
                    .RespectingRuntimeTypes()
                    .Using<DateTime>(ctx => ctx.Subject.ToUniversalTime().Should().BeCloseTo(ctx.Expectation.ToUniversalTime(), TimeSpan.FromMilliseconds(1)))
                    .WhenTypeIs<DateTime>());
            }, iter: 1000);
        }

        [TestMethod]
        public void Transit_Json_And_JsonVerbose_Are_Equivalent()
        {
            Generators.AnyGen.Sample(value => 
            {
                var json = TransitConvert.SerializeObject(value, TransitFactory.Format.Json);
                var verbose = TransitConvert.SerializeObject(value, TransitFactory.Format.JsonVerbose);

                var fromJson = TransitConvert.DeserializeObject<object>(json, TransitFactory.Format.Json);
                var fromVerbose = TransitConvert.DeserializeObject<object>(verbose, TransitFactory.Format.JsonVerbose);

                var normFromJson = Normalize(fromJson);
                var normFromVerbose = Normalize(fromVerbose);

                normFromJson.Should().BeEquivalentTo(normFromVerbose, options => options
                    .RespectingRuntimeTypes()
                    .Using<DateTime>(ctx => ctx.Subject.ToUniversalTime().Should().BeCloseTo(ctx.Expectation.ToUniversalTime(), TimeSpan.FromMilliseconds(1)))
                    .WhenTypeIs<DateTime>());
            }, iter: 1000);
        }
    }
}
