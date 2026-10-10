using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

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
    /// <summary>$97:8D12: blank tile $2F; letter tiles use palette four, without priority or reflection.</summary>
    private const ushort Blank = 0x002f;
    /// <summary>$97:8D12: selected BG3 palette four, with no priority or flip bits.</summary>
    private const ushort LetterStyle = 0x1000;
    /// <summary>$95:D089: uppercase A begins at tile $30, with the second alphabet block following both halves of A-P.</summary>
    private const int FirstLetterTile = 0x30;
    /// <summary>$95:D089: period tile $26 occupies only the lower glyph row.</summary>
    private const int PeriodTile = 0x26;

    /// <summary>Builds the opening narration's centered BG3 tilemap from the selected prose and compiled font layout.</summary>
    /// <returns>Little-endian BG3 page bytes containing the two-row glyphs and blank tiles elsewhere.</returns>
    internal static byte[] Compile()
    {
        int columns = IntroCinematicArtworkFormat.TileColumns;
        var output = new byte[IntroCinematicArtworkFormat.BackgroundPageByteCount];
        for (int index = 0; index < output.Length / 2; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(index * 2), Blank);
        int line = 0, column = 0, start = (columns - TextColumns) / 2;
        foreach (string word in Text.Split(' '))
        {
            if (column != 0 && column + 1 + word.Length > TextColumns) { line++; column = 0; }
            else if (column != 0) column++;
            foreach (char letter in word)
            {
                for (int half = 0; half < GlyphRows; half++)
                {
                    ushort value;
                    if (letter == '.') value = half == 0 ? Blank : (ushort)(LetterStyle | PeriodTile);
                    else
                    {
                        int ordinal = letter - 'A';
                        int tile = FirstLetterTile + ordinal + (ordinal / IntroFontAtlasFormat.TilesPerRow + half) * IntroFontAtlasFormat.TilesPerRow;
                        value = (ushort)(LetterStyle | tile);
                    }
                    int row = FirstRow + line * (GlyphRows + Leading) + half;
                    BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan((row * columns + start + column) * 2), value);
                }
                column++;
            }
        }
        return output;
    }
}
