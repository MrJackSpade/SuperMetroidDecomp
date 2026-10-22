using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1392 frame-composition rule, SME6442.</summary>
internal static class FrameCompositionAnalyzerTests
{
    /// <summary>Character pages and palette rows reachable from the draw entries, and the members the rule leaves alone.</summary>
    internal static void VerifyFrameCompositionAnalyzer()
    {
        var analyzer = new FrameCompositionAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        void Check(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : FrameCompositionAnalyzer.FrameCompositionId, message, analyzer, assemblyName: core);

        // A draw entry holding a character sheet composes its frame from characters.
        Check("""
            class SpriteArtwork { }
            class SpriteDraw { public SpriteArtwork Artwork { get; } = new(); }
            """, "'Artwork' holds a character page on a draw path; draw a pre-assembled, pre-coloured image selected by identity instead");
        // A kind of draw entry is a draw entry: its members are on the draw path.
        Check("""
            class SpriteArtwork { }
            abstract class SpriteDraw { }
            sealed class CharacterSpriteDraw : SpriteDraw { public SpriteArtwork Artwork { get; } = new(); }
            """, "'Artwork' holds a character page on a draw path; draw a pre-assembled, pre-coloured image selected by identity instead");
        // Reached through a collection of parts: a part selects its palette row by number.
        Check("""
            using System.Collections.Immutable;
            record struct SpriteFramePart(int Palette);
            class SpriteFrame { public ImmutableArray<SpriteFramePart> Parts { get; } }
            class SpriteDraw { public SpriteFrame Frame { get; } = new(); }
            """, "'Palette' selects a palette row on a draw path; draw a pre-assembled, pre-coloured image selected by identity instead");
        // The published frame carrying the colour memory.
        Check("""
            class SnesCgram { }
            class Memory { private readonly SnesCgram[] rows = []; }
            class RenderFrameSnapshot { public Memory? Memory { get; } }
            """, "'rows' holds palette rows on a draw path; draw a pre-assembled, pre-coloured image selected by identity instead");

        // A type the draw entries do not reach may keep its pages: an extraction-side or effect-side catalog.
        Check("""
            class SpriteArtwork { }
            class Catalog { public SpriteArtwork Sheet { get; } = new(); int Palette; }
            class SpriteDraw { public int Layer { get; } }
            """);
        // A part naming the character it starts at composes its frame from characters.
        Check("""
            record struct SpriteFramePart(int Character);
            class SpriteDraw { public SpriteFramePart Part { get; } }
            """, "'Character' selects characters on a draw path; draw a pre-assembled, pre-coloured image selected by identity instead");
        // An identity named for what it selects names finished artwork, not raw characters or a palette row.
        Check("""
            enum RoomCharacterSet { Upper }
            enum RoomPaletteSet { Upper }
            record struct TilesetDefinition(RoomCharacterSet Characters, RoomPaletteSet Palette);
            class SpriteDraw { public TilesetDefinition Tileset { get; } }
            """);
        // Names that merely contain the word are not palette-row selectors.
        Check("""
            class SpriteDraw { public string PaletteName { get; } = ""; public int Paletted { get; } }
            """);
        // Outside Core the import pipeline builds images from characters.
        VerifySnippet("""
            class SpriteArtwork { }
            class SpriteDraw { public SpriteArtwork Artwork { get; } = new(); }
            """, analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
