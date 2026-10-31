using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The opening BG3 card, decompressed from $97:8D12 and copied at $8B:A4C5 to VRAM word $4C00.</summary>
internal static class IntroInitialNarrationTilemapDefinitions
{
    /// <summary>$97:8D12: chosen English opening narration. Replacing the sentence invents different narrative content; only this prose/layout choice is retained, not timing or glyph pixels.</summary>
    internal const string Text = "THE LAST METROID IS IN CAPTIVITY. THE GALAXY IS AT PEACE...";
    /// <summary>$97:8D12: selected 22-column text measure; the left margin is calculated by centering it in the BG page.</summary>
    internal const int TextColumns = 22;
    /// <summary>$97:8D12: selected first glyph row eight, with one blank tile row between two-row glyphs.</summary>
    internal const int FirstRow = 8;
    /// <summary>$97:8D12: chosen one-row leading between successive two-row text lines.</summary>
    internal const int Leading = 1;
    /// <summary>$95:D089 font: 16 tiles per atlas row; two vertically stacked tiles per uppercase glyph.</summary>
    internal const int GlyphRows = 2;
    /// <summary>$97:8D12: blank tile $2F, in colour variant zero; letter tiles use colour variant four, without priority or reflection.</summary>
    internal static BackgroundTile Blank => new(47, 0, highPriority: false);
    /// <summary>$97:8D12: selected BG3 colour variant four, with no priority or mirroring.</summary>
    internal const int LetterPalette = 4;
    /// <summary>$95:D089: uppercase A begins at tile $30, with the second alphabet block following both halves of A-P.</summary>
    internal const int FirstLetterTile = 48;
    /// <summary>$95:D089: period tile $26 occupies only the lower glyph row.</summary>
    internal const int PeriodTile = 38;
}
