using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening narration's pictures once (#1467) from the narration font under the cinematic's palette, each pixel
/// in the priority tier its tile gave it: the first card over the whole font, as the cinematic shows it before the first
/// illustrated page; the final line and each letter in each glow colour over the font with its Japanese glyphs blanked, as
/// <c>BlankOut_JapanText_Tiles</c> leaves it for the pages. A tile naming a character the font does not hold shows nothing.
/// </summary>
internal static class IntroNarrationPictureSheetComposer
{
    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>Bytes in one two-bit font character.</summary>
    private const int CharacterBytes = IntroFontAtlasFormat.ByteCount / IntroFontAtlasFormat.TileCount;

    /// <summary>Composes the sheet from the installed intro artwork and narration font.</summary>
    internal static ComposedSpriteSheet Compose(IntroCinematicArtwork intro, IntroFontAtlas font)
    {
        Ensure.NotNull(intro);
        byte[] whole = new byte[IntroTileSetDefinitions.FontFirstTile * CharacterBytes + IntroFontAtlasFormat.ByteCount];
        IntroFontCharacters.Of(Ensure.NotNull(font)).Characters.Span[..IntroFontAtlasFormat.ByteCount].CopyTo(whole.AsSpan(IntroTileSetDefinitions.FontFirstTile * CharacterBytes));
        byte[] blanked = Blanked(whole);
        RgbColor[] colours = ScreenColourImage.Encode(intro.Palette.Colours)[..SpriteImage.SpriteColourCount].ToArray();
        int cells = IntroNarrationPictures.PlaneCells;
        int finalRows = IntroCinematicDefinitions.Text.FinalLineWordCount / cells;
        char[] letters = [.. IntroNarrationPictures.Letters];
        int variants = IntroNarrationPictures.LetterColourVariants;
        int letterTop = cells + finalRows;
        int letterRows = (letters.Length + cells - 1) / cells * variants;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(cells * CharacterPixels)), checked((ushort)((letterTop + letterRows) * CharacterPixels))));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [IntroNarrationPictures.FirstCard] = PaintRect(canvas, 0, 0, cells, intro.InitialNarrationTilemap.Span[..IntroCinematicDefinitions.Artwork.NarrationTilemapCells], whole, colours),
            [IntroNarrationPictures.FinalLine] = PaintRect(canvas, 0, cells, cells, intro.FinalLine.Tiles.Span[..IntroCinematicDefinitions.Text.FinalLineWordCount], blanked, colours),
        };
        for (int variant = 0; variant < variants; variant++)
        {
            for (int index = 0; index < letters.Length; index++)
            {
                BackgroundTile glyph = IntroNarrationGlyphTiles.CompileGlyph(letters[index]) with { ColourVariant = variant };
                int row = letterTop + variant * (letterRows / variants) + index / cells;
                frames.Add(IntroNarrationPictures.Letter(letters[index], variant), PaintRect(canvas, index % cells, row, 1, [glyph], blanked, colours));
            }
        }
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.FourColours));
    }

    /// <summary>
    /// <paramref name="font"/> with each Japanese glyph replaced by the font's own blank glyph, as
    /// <c>BlankOut_JapanText_Tiles</c> repeats it over them.
    /// </summary>
    private static byte[] Blanked(byte[] font)
    {
        byte[] blanked = (byte[])font.Clone();
        ReadOnlySpan<byte> blank = font.AsSpan(IntroTileSetDefinitions.FontFirstTile * CharacterBytes + IntroCinematicDefinitions.Text.JapaneseBlankSourceOffset, CharacterBytes);
        int first = IntroTileSetDefinitions.JapaneseBlankFirstTile * CharacterBytes;
        for (int offset = 0; offset < IntroCinematicDefinitions.Artwork.JapaneseBlankCharactersByteCount; offset += CharacterBytes)
        {
            if (first + offset + CharacterBytes > blanked.Length)
                throw new InvalidDataException($"The narration font ends before the Japanese glyph at byte {first + offset} it blanks.");
            blank.CopyTo(blanked.AsSpan(first + offset));
        }
        return blanked;
    }

    /// <summary>
    /// Paints <paramref name="tiles"/>, rows of <paramref name="columns"/> cells, from cell (<paramref name="left"/>,
    /// <paramref name="top"/>) over <paramref name="characters"/>, returning its frame.
    /// </summary>
    private static SpriteSheetFrameDocument PaintRect(RoomTileCanvas canvas, int left, int top, int columns, ReadOnlySpan<BackgroundTile> tiles,
        byte[] characters, RgbColor[] colours)
    {
        ImageColourGroups coloured = ImageColourGroups.None;
        for (int cell = 0; cell < tiles.Length; cell++)
        {
            int start = tiles[cell].Tile * CharacterBytes;
            if (start + CharacterBytes <= characters.Length)
                coloured = coloured.Union(canvas.PaintTwoBppTile((left + cell % columns) * CharacterPixels, (top + cell / columns) * CharacterPixels, tiles[cell],
                    characters.AsSpan(start, CharacterBytes), colours));
        }
        return new SpriteSheetFrameDocument
        {
            Corner = new Position16<ImagePixels>(checked((ushort)(left * CharacterPixels)), checked((ushort)(top * CharacterPixels))),
            Size = new Extent16<ImagePixels>(checked((ushort)(columns * CharacterPixels)), checked((ushort)(tiles.Length / columns * CharacterPixels))),
            Origin = default,
            ColourGroups = coloured.ManifestMembers,
            Parts = [],
        };
    }
}
