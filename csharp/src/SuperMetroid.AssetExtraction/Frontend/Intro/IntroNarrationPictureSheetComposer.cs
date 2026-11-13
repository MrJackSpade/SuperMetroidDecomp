using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening narration's pictures once (#1467) from the narration font under the cinematic's palette, each
/// installed whole (#1634): the first card over the whole font, as the cinematic shows it before the first
/// illustrated page; the final line and each letter, with a shade for each colour variant the letters' typing or glow gives
/// them (#1512), over the font with its Japanese glyphs blanked, as
/// <c>BlankOut_JapanText_Tiles</c> leaves it for the pages. A tile naming a character the font does not hold shows nothing.
/// </summary>
internal static class IntroNarrationPictureSheetComposer
{

    /// <summary>Each letter the narration types: its alphabet without the space, which types none.</summary>
    internal static IEnumerable<char> Letters => IntroNarrationGlyphs.Alphabet.Where(static letter => letter != ' ');

    /// <summary>The name of every picture the sheet holds.</summary>
    internal static IEnumerable<string> FrameNames =>
    [
        IntroNarrationPictures.FirstCard, IntroNarrationPictures.FinalLine,
        .. Letters.Select(IntroNarrationPictures.Letter),
    ];

    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>Bytes in one two-bit font character.</summary>
    private const int CharacterBytes = IntroFontAtlasFormat.ByteCount / IntroFontAtlasFormat.TileCount;

    /// <summary>Composes the sheet from the installed intro artwork and narration font.</summary>
    internal static ComposedSpriteSheet Compose(ImportIntroCinematicArtwork intro, IntroFontCharacters font)
    {
        Ensure.NotNull(intro);
        byte[] whole = new byte[IntroTileSetDefinitions.FontFirstTile * CharacterBytes + IntroFontAtlasFormat.ByteCount];
        Ensure.NotNull(font).Characters.Span[..IntroFontAtlasFormat.ByteCount].CopyTo(whole.AsSpan(IntroTileSetDefinitions.FontFirstTile * CharacterBytes));
        byte[] blanked = Blanked(whole);
        RgbColor[] colours = [.. intro.Palette.BackgroundColours];
        int cells = IntroNarrationPictures.PlaneCells;
        int finalRows = IntroArtworkExtentDefinitions.FinalLineWordCount / cells;
        char[] letters = [.. IntroNarrationPictureSheetComposer.Letters];
        int letterTop = cells + finalRows;
        int letterRows = (letters.Length + cells - 1) / cells;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(cells * CharacterPixels)), checked((ushort)((letterTop + letterRows) * CharacterPixels))));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [IntroNarrationPictures.FirstCard] = PaintRect(canvas, 0, 0, cells, intro.InitialNarrationTilemap.Span[..IntroArtworkExtentDefinitions.NarrationTilemapCells], whole, colours),
            [IntroNarrationPictures.FinalLine] = PaintRect(canvas, 0, cells, cells, intro.FinalLine.Tiles.Span[..IntroArtworkExtentDefinitions.FinalLineWordCount], blanked, colours),
        };
        // Each letter is composed in the first colour variant; the variant its typing or glow gives it is a shade (#1512).
        for (int index = 0; index < letters.Length; index++)
        {
            BackgroundTile glyph = IntroNarrationGlyphTiles.CompileGlyph(letters[index]) with { ColourVariant = 0 };
            frames.Add(IntroNarrationPictures.Letter(letters[index]), PaintRect(canvas, index % cells, letterTop + index / cells, 1, [glyph], blanked, colours));
        }
        ComposedSpriteSheet sheet = canvas.ToSheet(colours, frames, SpriteImageColours.TwoBppBackground);
        return sheet with { Manifest = sheet.Manifest with { Shades = SheetShade.Documents([.. IntroSheetShades.Scene(colours), .. IntroSheetShades.Glow(colours)]) } };
    }

    /// <summary>
    /// <paramref name="font"/> with each Japanese glyph replaced by the font's own blank glyph, as
    /// <c>BlankOut_JapanText_Tiles</c> repeats it over them.
    /// </summary>
    private static byte[] Blanked(byte[] font)
    {
        byte[] blanked = (byte[])font.Clone();
        ReadOnlySpan<byte> blank = font.AsSpan(IntroTileSetDefinitions.FontFirstTile * CharacterBytes + IntroArtworkExtentDefinitions.JapaneseBlankSourceOffset, CharacterBytes);
        int first = IntroTileSetDefinitions.JapaneseBlankFirstTile * CharacterBytes;
        for (int offset = 0; offset < IntroArtworkExtentDefinitions.JapaneseBlankCharactersByteCount; offset += CharacterBytes)
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
        };
    }
}
