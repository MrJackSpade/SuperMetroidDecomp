using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6476: in Core and the import pipeline, a sentinel number standing for absence and a placeholder member of an
/// identity are reported, while named constants, signs, comparisons' orders and library results are left alone (#2558).
/// </summary>
internal static class MagicAbsenceAnalyzerTests
{
    /// <summary>Each magic value the rule reports, and the values it leaves alone.</summary>
    internal static void VerifyMagicAbsenceAnalyzer()
    {
        var analyzer = new MagicAbsenceAnalyzer();
        const string Id = MagicAbsenceAnalyzer.MagicAbsenceId;
        void Expect(string body, string assembly = CoreLayerAnalyzer.CoreAssembly, params string[] ids) =>
            VerifySnippetIds("namespace SuperMetroid.Core.World { using System; using System.Linq; class Probe { " + body + " } }",
                analyzer, assembly, ids);
        void ExpectType(string declaration, string assembly = CoreLayerAnalyzer.CoreAssembly, params string[] ids) =>
            VerifySnippetIds("namespace SuperMetroid.Core.World { " + declaration + " }", analyzer, assembly, ids);

        // A filled sentinel, a sentinel beside a computed value, a returned sentinel and a seeded member are reported, in
        // Core and in the import pipeline.
        Expect("void Clear(int[] places) => Array.Fill(places, -1);", ids: [Id]);
        Expect("int[] Seeds() => Enumerable.Repeat(int.MaxValue, 4).ToArray();", ids: [Id]);
        Expect("int Find(int[] list, bool found, int x) => found ? Array.IndexOf(list, x) : -1;", ids: [Id]);
        Expect("int Or(int? x) => x ?? -1;", ids: [Id]);
        Expect("int Last(int[] parts) { if (parts.Length == 0) return -1; return parts[^1]; }", ids: [Id]);
        Expect("int CommonSheet { get; set; } = -1;", ids: [Id]);
        Expect("void Clear(int[] places) => Array.Fill(places, -1);", "SuperMetroid.AssetExtraction", ids: [Id]);
        // A local seeded with a sentinel that its body tests against zero or minus one is reported.
        Expect("int Pick(int[] values) { int selected = -1; foreach (int v in values) if (selected < 0 || v > values[selected]) selected = v; return selected; }",
            ids: [Id]);
        Expect("bool Any(int[] values) { int last = -1; foreach (int v in values) last = v; return last != -1; }", ids: [Id]);
        // A placeholder member of an identity enum, and a placeholder static of an identity or type value, are reported.
        ExpectType("enum RoomThingId { None, Crate, Door }", ids: [Id]);
        ExpectType("enum PictureId { Blank, Tree }", ids: [Id]);
        ExpectType("readonly record struct ShotType(int Value) { public static ShotType None => default; }", ids: [Id]);

        // A named constant, a sign beside another constant, a sign set in a body that never tests it against zero, a
        // comparison's order, a library's result read at its boundary and another assembly's sentinel are left alone.
        Expect("const int StepLeft = -1; int Step() => StepLeft;");
        Expect("int Sign(bool left) => left ? -1 : 1;");
        Expect("int Turn(bool left) { int direction = 1; if (left) direction = -1; return direction * 4; }");
        Expect("int CompareTo(Probe other) => -1;");
        Expect("bool Has(int[] list, int x) { int index = Array.IndexOf(list, x); return index >= 0; }");
        Expect("void Clear(int[] places) => Array.Fill(places, -1);", "SuperMetroid.Verification");
        Expect("int Offset(int x) => x - 1;");
        // A placeholder name on a flags enum, on an enum that is no identity, and a value of another type are left alone.
        ExpectType("[System.Flags] enum ButtonId { None = 0, A = 1 }");
        ExpectType("enum ShotReaction { Nothing, Breaks }");
        ExpectType("readonly record struct PixelFraction(int Value) { public static PixelFraction None => default; }");

        Console.WriteLine("Magic absence rule: filled, chosen, coalesced, returned and seeded sentinels, locals seeded with one their " +
            "body tests for, placeholder identity members and placeholder statics reported; named constants, signs, comparisons, " +
            "library results, flags enums, other enums and other assemblies left alone.");
    }
}
