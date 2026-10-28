using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's memory image into the BG3 plane a frame shows (#1403): each distinct tilemap word in the gameplay BG3
/// tilemap is painted, mirrored, coloured and prioritised as the word says, from its two-bit character, as extraction
/// paints a room's BG3 sheet, and each cell shows its word's picture.
/// </summary>
internal static class Layer3CellsFixtures
{
    /// <summary>The plane of a blank memory image: every cell character zero's transparent tile.</summary>
    private static readonly Lazy<Layer3Cells> blank = new(() =>
        FromVram(new byte[SnesPpuLayout.VramByteCount], new RgbColor[SnesPpuLayout.CgramColorCount]));

    /// <summary>The plane of a blank memory image: every cell character zero's transparent tile.</summary>
    internal static Layer3Cells Blank => blank.Value;

    /// <summary>
    /// The plane the gameplay BG3 tilemap in <paramref name="image"/> shows, its words' characters read from
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: reads a fixture's BG3 tilemap")]
    internal static Layer3Cells FromVram(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord = GameplayVramLayout.HudCharacterWord)
    {
        var words = new ushort[Layer3Plane.Columns * Layer3Plane.Rows];
        for (int cell = 0; cell < words.Length; cell++)
        {
            int at = SnesVramAddressingConstants.ByteOffsetOfWord(GameplayVramLayoutConstants.HudTilemapWord + cell);
            words[cell] = LittleEndianBytes.Word(image[at], image[at + 1]);
        }
        return FromWords(words, image, palette, characterWord);
    }

    /// <summary>
    /// The plane <paramref name="words"/> show, their characters read from <paramref name="characterWord"/> in
    /// <paramref name="image"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: decodes a BG3 plane's two-bit characters into the cells a frame shows")]
    internal static Layer3Cells FromWords(ReadOnlySpan<ushort> words, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord)
    {
        const int columns = 32, tile = 8, characterWords = 8;
        Ensure.LengthEqual(words, Layer3Plane.Columns * Layer3Plane.Rows);
        ushort[] distinct = [.. words.ToArray().Distinct()];
        var places = new Dictionary<ushort, int>();
        for (int place = 0; place < distinct.Length; place++)
            places.Add(distinct[place], place);
        int rows = (distinct.Length + columns - 1) / columns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * tile, checked((ushort)(rows * tile))));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        for (int place = 0; place < distinct.Length; place++)
        {
            var word = new SnesBgTilemapWord(distinct[place]);
            for (int part = 0; part < characterWords; part++)
            {
                int at = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word.CharacterIndex * characterWords + part);
                character[part * 2] = image[at];
                character[part * 2 + 1] = image[at + 1];
            }
            canvas.PaintTwoBppTile(place % columns * tile, place / columns * tile, word, character, colours);
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            ["tiles"] = TileWordSheetComposer.Frame(canvas, 0, rows, TileWordLayer.Bg3),
        };
        ComposedSpriteSheet composed = canvas.ToSheet(colours, frames, SpriteImageColours.TwoBppBackground);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "bg3-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartRows);
        SpriteSheetFrame picture = sheet.Frame("tiles");
        var pictures = new BgTilePicture?[words.Length];
        for (int cell = 0; cell < words.Length; cell++)
            pictures[cell] = new BgTilePicture(picture, new Position16<ImagePixels>((ushort)(places[words[cell]] % columns * tile),
                (ushort)(places[words[cell]] / columns * tile)));
        return new Layer3Cells([.. pictures]);
    }
}
