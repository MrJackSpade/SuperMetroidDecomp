using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening BG3 card, decompressed from $97:8D12 and copied at $8B:A4C5 to VRAM word $4C00.</summary>
internal static class IntroInitialNarrationTilemapDefinitions
{
    /// <summary>$97:8D12: chosen English opening narration. Replacing the sentence invents different narrative content; only this prose/layout choice is retained, not timing or glyph pixels.</summary>
    private const string Text = "THE LAST METROID IS IN CAPTIVITY. THE GALAXY IS AT PEACE...";
    /// <summary>$97:8D12: selected 22-column text measure; the left margin is calculated by centering it in the BG page.</summary>
    private const int TextColumns = 22;
    /// <summary>$97:8D12: selected first glyph row eight, with one blank tile row between two-row glyphs.</summary>
    private const int FirstRow = 8;
    /// <summary>$97:8D12: chosen one-row leading between successive two-row text lines.</summary>
    private const int Leading = 1;
    /// <summary>$95:D089 font: 16 tiles per atlas row; two vertically stacked tiles per uppercase glyph.</summary>
    private const int GlyphRows = 2;
    /// <summary>$97:8D12: blank tile $2F, in colour variant zero; letter tiles use colour variant four, without priority or reflection.</summary>
    private static BackgroundTile Blank => new(0x02f, 0, highPriority: false);
    /// <summary>$97:8D12: selected BG3 colour variant four, with no priority or mirroring.</summary>
    private const int LetterPalette = 4;
    /// <summary>$95:D089: uppercase A begins at tile $30, with the second alphabet block following both halves of A-P.</summary>
    private const int FirstLetterTile = 0x30;
    /// <summary>$95:D089: period tile $26 occupies only the lower glyph row.</summary>
    private const int PeriodTile = 0x26;

    /// <summary>Centers and wraps the selected English sentence into two-tile glyph rows on a blank BG3 page.</summary>
    internal static BackgroundTile[] Compile()
    {
        int columns = IntroCinematicArtworkFormat.TileColumns;
        var output = new BackgroundTile[IntroCinematicArtworkFormat.BackgroundPageByteCount / sizeof(ushort)];
        Array.Fill(output, Blank);
        int line = 0, column = 0, start = (columns - TextColumns) / 2;
        foreach (string word in Text.Split(' '))
        {
            if (column != 0 && column + 1 + word.Length > TextColumns) { line++; column = 0; }
            else if (column != 0) column++;
            foreach (char letter in word)
            {
                for (int half = 0; half < GlyphRows; half++)
                {
                    BackgroundTile value;
                    if (letter == '.') value = half == 0 ? Blank : new BackgroundTile(PeriodTile, LetterPalette, highPriority: false);
                    else
                    {
                        int ordinal = letter - 'A';
                        int tile = FirstLetterTile + ordinal + (ordinal / IntroFontAtlasFormat.TilesPerRow + half) * IntroFontAtlasFormat.TilesPerRow;
                        value = new BackgroundTile(tile, LetterPalette, highPriority: false);
                    }
                    int row = FirstRow + line * (GlyphRows + Leading) + half;
                    output[row * columns + start + column] = value;
                }
                column++;
            }
        }
        return output;
    }
}
