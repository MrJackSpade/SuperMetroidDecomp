using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1405 palette-layout rule, SME6444.</summary>
internal static class PaletteLayoutAnalyzerTests
{
    /// <summary>Colours laid out as palette memory, and the colour members the rule leaves alone.</summary>
    internal static void VerifyPaletteLayoutAnalyzer()
    {
        var analyzer = new PaletteLayoutAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        const string colour = "readonly record struct RgbColor(int Red, int Green, int Blue);";
        void Check(string source, string? message = null) =>
            VerifySnippet(colour + source, message is null ? null : PaletteLayoutAnalyzer.PaletteLayoutId, message, analyzer, assemblyName: core);
        const string advice = "; images are pre-coloured, so hold the effect's typed state or a domain-named colour list instead";

        // Colours named as a palette.
        Check("""
            class Boss { private readonly RgbColor[] bodyPalette = []; }
            """, "'bodyPalette' holds colours named as a palette" + advice);
        // A span of them is the same palette.
        Check("""
            class Screen { public System.ReadOnlySpan<RgbColor> Palettes => default; }
            """, "'Palettes' holds colours named as a palette" + advice);
        // Colours keyed by an integer slot.
        Check("""
            class Flight { private readonly System.Collections.Generic.Dictionary<int, RgbColor> edits = []; }
            """, "'edits' keys colours by an integer slot" + advice);

        // An effect's colour list with a domain name.
        Check("""
            class HitFlash { public System.Collections.Immutable.ImmutableArray<RgbColor> FlashColours { get; } }
            """);
        // The colours a fade steps through, one list per step.
        Check("""
            class BodyFade { private readonly RgbColor[][] steps = []; }
            """);
        // A slot-keyed table is still keyed by slot.
        Check("""
            class Flight { private readonly System.Collections.Generic.Dictionary<ushort, RgbColor[]> rowsBySlot = []; }
            """, "'rowsBySlot' keys colours by an integer slot" + advice);
        // One colour named for its role, even beside a palette word.
        Check("""
            class Kraid { public RgbColor EyeGlow { get; } public RgbColor PaletteFlash { get; } }
            """);
        // Colours keyed by an identity, not a slot.
        Check("""
            enum Area { Crateria }
            class Map { private readonly System.Collections.Generic.Dictionary<Area, RgbColor[]> world = []; }
            """);
        // A palette word on something that holds no colours.
        Check("""
            enum PaletteId { Escape }
            class Ending { public PaletteId Palette { get; } public int[] PaletteSteps { get; } = []; }
            """);
        // Outside Core the import pipeline reads colour memory.
        VerifySnippet(colour + """
            class Importer { private readonly RgbColor[] cgramPalette = []; }
            """, analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
