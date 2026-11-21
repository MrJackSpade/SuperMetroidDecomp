using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6487: in Core and the import pipeline, a Zip of two collections, an element read at a position found in
/// another collection, a projection whose index reads another collection, and a loop reading a static table and another
/// collection at its own index are reported, while positions within one sequence, keyed lookups, owners and other
/// assemblies are left alone (#2565).
/// </summary>
internal static class ParallelListAnalyzerTests
{
    /// <summary>Each pairing by position the rule reports, and the positions it leaves alone.</summary>
    internal static void VerifyParallelListAnalyzer()
    {
        var analyzer = new ParallelListAnalyzer();
        const string Id = ParallelListAnalyzer.ParallelListId;
        const string Types = "using System; using System.Linq; using System.Collections.Generic; " +
            "sealed class EncodingOwnerAttribute(string format) : Attribute { public string Format { get; } = format; } " +
            "sealed class CompatibilityServiceAttribute(string behaviour) : Attribute { public string Behaviour { get; } = behaviour; } " +
            "static class Table { public static readonly int[] Shades = [1, 2, 3]; public static readonly string[] Names = [\"a\", \"b\", \"c\"]; } ";
        void Expect(string body, string assembly = CoreLayerAnalyzer.CoreAssembly, params string[] ids) =>
            VerifySnippetIds("namespace SuperMetroid.Core.World { " + Types + "class Probe { " + body + " } }", analyzer, assembly, ids);

        // Two collections zipped, read at a position found in another, read at a projection's index, or read with a static
        // table at a loop's index are reported, in Core and the import pipeline.
        Expect("int[] Pair(int[] frames, int[] durations) => [.. frames.Zip(durations, (frame, duration) => frame + duration)];", ids: [Id]);
        Expect("int Shade(string name) => Table.Shades[Array.IndexOf(Table.Names, name)];", ids: [Id]);
        Expect("int Shade(List<string> names, string name) { int place = names.IndexOf(name); return Table.Shades[place]; }", ids: [Id]);
        Expect("int[] Looks(string[] looks, int[] uploads) => [.. looks.Select((look, place) => uploads[place])];", ids: [Id]);
        Expect("int Sum(int[] colours) { int sum = 0; for (int i = 0; i < colours.Length; i++) sum += Table.Shades[i] + colours[i]; return sum; }", ids: [Id]);
        Expect("int[] Pair(int[] frames, int[] durations) => [.. frames.Zip(durations, (frame, duration) => frame + duration)];", "SuperMetroid.AssetExtraction", ids: [Id]);

        // Neighbouring entries of one list, one list read at a position found in itself, a list written at the index another
        // is read from, a loop reading one list, keyed lookups, owners and other assemblies are left alone.
        Expect("int[] Steps(int[] path) => [.. path.Zip(path.Skip(1), (from, to) => to - from)];");
        Expect("int Next(List<int> cycle, int step) => cycle[cycle.IndexOf(step) + 1 == cycle.Count ? 0 : cycle.IndexOf(step) + 1];");
        Expect("int[] Doubled(int[] source) { var result = new int[source.Length]; for (int i = 0; i < source.Length; i++) result[i] = source[i] * 2; return result; }");
        Expect("int Sum() { int sum = 0; for (int i = 0; i < Table.Shades.Length; i++) sum += Table.Shades[i]; return sum; }");
        Expect("int Shade(Dictionary<string, int> shades, string name) => shades[name];");
        Expect("string[] Named(string[] names) => [.. names.Select((name, place) => name + place)];");
        Expect("int[] Pair(int[] frames, int[] durations) => [.. frames.Zip(durations, (frame, duration) => frame + duration)];", "SuperMetroid.Verification");
        VerifySnippetIds("namespace SuperMetroid.Core.World { " + Types + "[EncodingOwner(\"a layout\")] static class Layout { " +
            "public static int Shade(string name) => Table.Shades[Array.IndexOf(Table.Names, name)]; } }", analyzer, CoreLayerAnalyzer.CoreAssembly);
        VerifySnippetIds("namespace SuperMetroid.Core.World { " + Types + "[CompatibilityService(\"a placement\")] static class Placement { " +
            "public static int[] Pair(int[] places, int[] shots) => [.. places.Zip(shots, (place, shot) => place + shot)]; } }", analyzer, CoreLayerAnalyzer.CoreAssembly);

        Console.WriteLine("Parallel list rule: zips, positions found in another collection, projection indices and static tables read " +
            "at a loop's index reported; one sequence's positions, keyed lookups, owners and other assemblies left alone.");
    }
}
