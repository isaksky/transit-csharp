using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CsCheck;
using Transit;

namespace Transit.Net.Tests
{
    public static class Generators
    {
        // --- PRIMITIVES ---
        public static Gen<int> IntGen => Gen.Int;
        public static Gen<long> LongGen => Gen.Long;
        public static Gen<double> DoubleGen => Gen.Double;
        public static Gen<bool> BoolGen => Gen.Bool;
        
        // Let's use alphanumeric strings for now to avoid weird surrogates breaking naive JSON parsers
        public static Gen<string> StringGen => Gen.String.AlphaNumeric;

        // --- EXOTIC SCALARS ---
        // Identifiers
        public static Gen<IKeyword> KeywordGen => 
            Gen.String.AlphaNumeric.Select(s => TransitFactory.Keyword(string.IsNullOrEmpty(s) ? "k" : s));

        public static Gen<ISymbol> SymbolGen => 
            Gen.String.AlphaNumeric.Select(s => TransitFactory.Symbol(string.IsNullOrEmpty(s) ? "s" : s));

        // BigInt
        public static Gen<BigInteger> BigIntegerGen => 
            Gen.Byte.Array.Select(b => new BigInteger(b));

        public static Gen<Guid> GuidGen => Gen.Guid;
        
        // URIs
        public static Gen<Uri> UriGen => 
            Gen.String.AlphaNumeric.Select(s => new Uri($"http://example.com/{(string.IsNullOrEmpty(s) ? "u" : s)}"));

        // Date and Time
        // Transit truncates to milliseconds or ticks but usually we just care about ms precision
        // We set kind to Local because Transit in C# currently deserializes to Local time by default.
        public static Gen<DateTime> DateTimeGen => 
            Gen.DateTime.Select(d => new DateTime(d.Ticks - (d.Ticks % 10000), DateTimeKind.Local));

        public static Gen<DateTimeOffset> DateTimeOffsetGen => 
            Gen.DateTimeOffset.Select(d => new DateTimeOffset(d.Ticks - (d.Ticks % 10000), TimeZoneInfo.Local.GetUtcOffset(d.UtcDateTime)));


        // --- COMPOSITE TYPES ---
        
        // A combined generator for basic scalar types
        public static Gen<object> ScalarGen => Gen.OneOf<object>(
            IntGen.Select(x => (object)x),
            LongGen.Select(x => (object)x),
            DoubleGen.Select(x => (object)x),
            BoolGen.Select(x => (object)x),
            StringGen.Select(x => (object)x),
            KeywordGen.Select(x => (object)x),
            SymbolGen.Select(x => (object)x),
            GuidGen.Select(x => (object)x),
            DateTimeGen.Select(x => (object)x)
        );
        
        // We omit DateTime from dictionary keys to prevent Transit/CSharp EqualityComparer lookup failures.
        public static Gen<object> DictKeyGen => Gen.OneOf<object>(
            IntGen.Select(x => (object)x),
            LongGen.Select(x => (object)x),
            DoubleGen.Select(x => (object)x),
            BoolGen.Select(x => (object)x),
            StringGen.Select(x => (object)x),
            KeywordGen.Select(x => (object)x),
            SymbolGen.Select(x => (object)x),
            GuidGen.Select(x => (object)x)
        );

        // Depth-bounded Generator for complex objects 
        public static Gen<object> AnyGen => Gen.OneOf<object>(
            ScalarGen,
            ScalarGen.Array.Select(a => (object)a), // object[]
            Gen.Dictionary(DictKeyGen, ScalarGen).Select(d => (object)d) // Dictionary<object, object>
        );
    }
}
