<Query Kind="Program">
  <NuGetReference>CsCheck</NuGetReference>
  <Namespace>CsCheck</Namespace>
</Query>

#:project "../src/Transit/Transit.csproj"
#:project "../src/Transit.Tests/Transit.Tests.csproj"

void Main() {
    Console.WriteLine("--- SAMPLE C# OBJECTS SERIALIZED WITH NEWTONSOFT.JSON ---");
    Console.WriteLine();

    var samples = new List<object>();

    Transit.Tests.Generators.AnyGen.Sample(x => {
        samples.Add(x);
    }, iter: 15);

    int i = 1;
    foreach (var sample in samples) {
        try {
            sample.Dump($"Sample {i + 1}");
        } catch (Exception ex) {
            Console.WriteLine($"Sample {i} failed to serialize because: {ex.Message}");
        }

        Console.WriteLine();
        i++;
    }
}
