using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6486: gameplay code names a random roll by its outcomes or reads a compatibility service's member for it,
/// and only a compatibility service builds a random sample field from a position and a width (#2563).
/// </summary>
internal static class RandomPlaceSelectionAnalyzerTests
{
    /// <summary>The compatibility types the cases read, declared as Core declares them.</summary>
    private const string Declarations =
        "namespace SuperMetroid.Core.Compatibility { [System.AttributeUsage(System.AttributeTargets.All)]" +
        " public sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }" +
        " [CompatibilityService(\"which places of the random number each choice reads\")]" +
        " public readonly record struct RandomSampleField(int Shift, int Width) {" +
        " public static RandomSampleField Whole { get; } = new(0, 16); public static RandomSampleField Nothing { get; } = new(0, 0);" +
        " public static RandomSampleField OneOf(int n) => new(0, System.Numerics.BitOperations.Log2((uint)n));" +
        " public static RandomSampleField SecondOf(int n) => new(8, System.Numerics.BitOperations.Log2((uint)n));" +
        " public static RandomSampleField RangePart(int n) => new(16 - System.Numerics.BitOperations.Log2((uint)n), System.Numerics.BitOperations.Log2((uint)n)); }" +
        " public readonly record struct RandomSpeed(RandomSampleField Field, int Slowest); }";

    /// <summary>Each construction the rule reports outside a compatibility service, and the rolls it leaves alone.</summary>
    internal static void VerifyRandomPlaceSelectionAnalyzer()
    {
        var analyzer = new RandomPlaceSelectionAnalyzer();
        void Expect(string body, string? built = null, string assembly = CoreLayerAnalyzer.CoreAssembly, string attributes = "") =>
            VerifySnippet(Declarations + " namespace SuperMetroid.Core.Bosses { using SuperMetroid.Core.Compatibility; " + attributes +
                " static class Probe { " + body + " } }",
                built is null ? null : RandomPlaceSelectionAnalyzer.RandomPlaceSelectionId,
                built is null ? null : $"'{built}' selects places of the random number by position and width; name the roll (RandomSampleField.OneOf, SecondOf or RangePart) or a compatibility service's member for the choice",
                analyzer, assemblyName: assembly);

        // A field built by name, target-typed in a declaration or an argument, or copied with new places is reported.
        Expect("static readonly RandomSampleField Drop = new RandomSampleField(8, 6);", "new RandomSampleField(8, 6)");
        Expect("static readonly RandomSampleField Drop = new(0, 7);", "new(0, 7)");
        Expect("static readonly RandomSpeed Fall = new(new(0, 6), 2);", "new(0, 6)");
        Expect("static RandomSampleField Wider(RandomSampleField f) => f with { Width = 7 };", "f with { Width = 7 }");
        Expect("static int Read(int sample) => (sample >> new RandomSampleField(3, 2).Shift) & 3;", "new RandomSampleField(3, 2)");
        // A compatibility service, and a type nested in one, builds the places the game read.
        Expect("static readonly RandomSampleField Walk = new(3, 2);", attributes: "[CompatibilityService(\"Botwoon's walk\")]");
        Expect("static class Inner { static readonly RandomSampleField Walk = new(3, 2); }", attributes: "[CompatibilityService(\"Botwoon's walk\")]");
        // Rolls named by their outcomes, the whole number, nothing, a field passed on and other assemblies are not reported.
        Expect("static readonly RandomSampleField Drop = RandomSampleField.OneOf(128);");
        Expect("static readonly RandomSampleField Height = RandomSampleField.SecondOf(64);");
        Expect("static readonly RandomSampleField Half = RandomSampleField.RangePart(2);");
        Expect("static readonly RandomSampleField All = RandomSampleField.Whole; static readonly RandomSampleField None = RandomSampleField.Nothing;");
        Expect("static RandomSpeed Fall(RandomSampleField f) => new(f, 2);");
        Expect("static readonly RandomSampleField Drop = new(0, 7);", assembly: "SuperMetroid.Verification");
        Console.WriteLine("Random place selection rule: a RandomSampleField built from a position and width by name, target-typed or by a with copy reported outside a compatibility service; services, types nested in them, the named rolls, the whole number, nothing, a field passed on and other assemblies left alone.");
    }
}
