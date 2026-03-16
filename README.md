# transit-csharp

Transit is a data format and a set of libraries for conveying values between applications written in different languages. This library provides support for marshalling Transit data to/from C#.

* [Rationale](http://blog.cognitect.com/blog/2014/7/22/transit)
* [Specification](http://github.com/cognitect/transit-format)

This implementation's major.minor version number corresponds to the version of the Transit specification it supports.

JSON and JSON-Verbose formats are implemented. MessagePack is **not** implemented.

> **Note:** This is a modernized fork of [rickbeerendonk/transit-csharp](https://github.com/rickbeerendonk/transit-csharp), ported to .NET 10 with a focus on performance and modern C# idioms. The original implementation is preserved in `src_old/`.

---

## What Changed

### Platform

| | Original | This fork |
|---|---|---|
| Target framework | .NET Framework 4.5 / PCL | .NET 10 |
| JSON engine | Newtonsoft.Json | `System.Text.Json` (`Utf8JsonWriter` / `Utf8JsonReader`) |
| Namespace | `Beerendonk.Transit` | `Transit` |

### Write path

* **`Utf8JsonWriter` + `ArrayBufferWriter<byte>`** — the emitter writes directly into a pooled byte buffer; the buffer is then copied to the destination `Stream` in one shot and reset. No intermediate `MemoryStream` or `TextWriter` allocation.
* **`FrozenDictionary<Type, IWriteHandler>`** — handler tables are built once at startup and stored in a `FrozenDictionary` for minimal lookup cost.
* **`ConcurrentDictionary` handler cache** — types not directly registered (subclasses, interface implementations) are resolved by a one-time walk of the type hierarchy; the result is memoised so subsequent serialisations of the same type are O(1).
* Typed fast paths for primitive element sequences (`IEnumerable<int>`, `IEnumerable<long>`, `IEnumerable<double>`, etc.) avoid boxing in homogeneous arrays.
* `string.Create` used in `WriteCache` and `AbstractEmitter.Escape` to build escaped strings without intermediate allocations.
* `IWriter<T>` now implements `IDisposable`; the `Utf8JsonWriter` and (optionally) the underlying `Stream` are disposed properly. The `ownsStream` parameter controls whether the writer takes ownership of the stream.

### Read path

* **`Utf8JsonReader`** (a `ref struct`) — streaming, zero-copy JSON parsing directly over the raw UTF-8 byte buffer returned by the stream.
* **`FrozenDictionary<string, IReadHandler>`** — same pattern as the write side.
* **`NullKeyDictionary`** — a bespoke `IDictionary` implementation that permits `null` keys, matching Java `HashMap` semantics required for the `cmap` (composite-map) transit type.
* Fast path for `MemoryStream` inputs: avoids a redundant `CopyTo` by accessing the internal buffer segment via `TryGetBuffer`.

### Type mapping

|Transit type|Write accepts|Read returns|
|------------|-------------|------------|
|null|`null`|`null`|
|string|`System.String`|`System.String`|
|boolean|`System.Boolean`|`System.Boolean`|
|integer|`byte`, `short`, `int`, `long`|`System.Int64`|
|decimal|`float`, `double`|`System.Double`|
|keyword|`Transit.IKeyword`|`Transit.IKeyword`|
|symbol|`Transit.ISymbol`|`Transit.ISymbol`|
|big decimal|`System.Decimal`, `Transit.Numerics.BigRational`|`Transit.Numerics.BigRational`|
|big integer|`System.Numerics.BigInteger`|`System.Numerics.BigInteger`|
|time|`System.DateTime`|`System.DateTime`|
|uri|`System.Uri`|`System.Uri`|
|uuid|`System.Guid`|`System.Guid`|
|char|`System.Char`|`System.Char`|
|array|`T[]`, `IList<>`|`IList<object>`|
|list|`IEnumerable<>`|`IEnumerable<object>`|
|set|`ISet<>`|`ISet<object>`|
|map|`IDictionary<,>`|`IDictionary<object, object>`|
|link|`Transit.ILink`|`Transit.ILink`|
|ratio †|`Transit.IRatio`|`Transit.IRatio`|
|cmap (composite keys)|`IDictionary<,>` with non-string keys|`Transit.Impl.NullKeyDictionary`|

† Extension type

---

## Usage

```csharp
// Write
using var stream = new MemoryStream();
using var writer = TransitFactory.Writer<object>(TransitFactory.Format.Json, stream, ownsStream: false);
writer.Write(myObject);
byte[] bytes = stream.ToArray();

// Read
using var readStream = new MemoryStream(bytes);
var reader = TransitFactory.Reader(TransitFactory.Format.Json, readStream);
var value = reader.Read<object>();
```

### Custom handlers

```csharp
// Custom write handler
var customWriteHandlers = new Dictionary<Type, IWriteHandler>
{
    [typeof(MyType)] = new MyWriteHandler()
};
using var writer = TransitFactory.Writer<object>(TransitFactory.Format.Json, stream, customWriteHandlers);

// Custom read handler
var customReadHandlers = new Dictionary<string, IReadHandler>
{
    ["my-tag"] = new MyReadHandler()
};
var reader = TransitFactory.Reader(TransitFactory.Format.Json, stream, customReadHandlers);

// Catch-all Default Write Handler (called for unregistered types)
using var w = TransitFactory.Writer<object>(TransitFactory.Format.Json, stream, null, myDefaultHandler);

// Write-time Transform (modify objects before handler lookup)
Func<object, object> transform = obj => obj is Point p ? new[] { p.X, p.Y } : obj;
using var tw = TransitFactory.Writer<object>(TransitFactory.Format.Json, stream, null, null, transform);
```

### High-performance Pre-merged Handlers

If you are creating many readers and writers with custom handlers, you can pre-merge them once to avoid dictionary-merge allocations on every instantiation:

```csharp
var mergedWriteHandlers = TransitFactory.MergedWriteHandlers(customWriteHandlers);
var mergedReadHandlers = TransitFactory.MergedReadHandlers(customReadHandlers);

// Use the pre-merged frozen maps directly
using var writer = TransitFactory.Writer<object>(TransitFactory.Format.Json, stream, mergedWriteHandlers);
using var reader = TransitFactory.Reader(TransitFactory.Format.Json, stream, mergedReadHandlers);
```

---

## Building & Testing

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
# Build
dotnet build src/

# Run tests
dotnet test src/Transit.Tests/

# Run benchmarks (compare old vs new)
dotnet run --project src/Transit.Benchmarks/ -c Release
```

---

## Benchmarks

The `Transit.Benchmarks` project uses [BenchmarkDotNet](https://benchmarkdotnet.org/) and references both the original and new implementations side-by-side, measuring throughput and allocations for read, write, and cache-encoding workloads.

---

## Project Structure

```
src/
  Transit/               # Library (this fork)
    Impl/                # Internal emitters, parsers, caches, handlers
    Numerics/            # BigRational type
    Spi/                 # Extension SPI (IReaderSpi, etc.)
  Transit.Tests/         # Test suite (covers exemplar files from transit-format)
  Transit.Benchmarks/    # BenchmarkDotNet comparison harness

src_old/                 # Original Beerendonk implementation (reference)
transit-format/          # Upstream Transit specification & exemplar files
transit-java/            # Java reference implementation (submodule)
```

---

## Copyright and License

Copyright © 2014 Rick Beerendonk.

This library is a C# port of the Java version created and maintained by Cognitect, therefore

Copyright © 2014 Cognitect

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at  
http://www.apache.org/licenses/LICENSE-2.0  
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
