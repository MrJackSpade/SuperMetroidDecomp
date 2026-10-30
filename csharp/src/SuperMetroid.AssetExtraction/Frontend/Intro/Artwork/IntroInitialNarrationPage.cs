using static SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroInitialNarrationTilemapDefinitions;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Draws the intro's opening narration page (<see cref="IntroInitialNarrationTilemapDefinitions"/>).
/// </summary>
internal static class IntroInitialNarrationPage
{
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
