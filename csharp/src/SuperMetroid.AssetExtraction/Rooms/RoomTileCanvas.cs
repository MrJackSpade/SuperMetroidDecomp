using SuperMetroid.Core.Spatial;
using System.Buffers.Binary;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The pixel planes of a pre-coloured room sheet, painted one four-bpp tile at a time over a tileset's background colours
/// (#1392): each pixel's colour, palette index, tile priority and row.
/// </summary>
/// <param name="size">The sheet's size.</param>
internal sealed class RoomTileCanvas(Extent16<ImagePixels> size)
{
    /// <summary>Colours in one palette row.</summary>
    private const int RowColours = 16;

    /// <summary>Pixels along a character's side.</summary>
    internal const int CharacterPixels = 8;

    /// <summary>Bytes in one four-bpp character.</summary>
    internal const int CharacterBytes = 32;

    /// <summary>The sheet's size.</summary>
    internal Extent16<ImagePixels> Size { get; } = size;

    /// <summary>The sheet's width: the pixels from one row to the next.</summary>
    internal int Width => Size.X;

    /// <summary>Each pixel's colour.</summary>
    internal RgbaPixel[] Pixels { get; } = new RgbaPixel[size.X * size.Y];

    /// <summary>Each pixel's palette row times sixteen plus its colour; zero where transparent.</summary>
    internal byte[] PaletteIndices { get; } = new byte[size.X * size.Y];

    /// <summary>Each pixel's tile priority.</summary>
    internal byte[] Priorities { get; } = new byte[size.X * size.Y];

    /// <summary>Each pixel's row, which no draw overrides.</summary>
    internal byte[] PartRows { get; } = new byte[size.X * size.Y];

    /// <summary>The 128 background colours a tileset's palette loads, the first half of CGRAM.</summary>
    internal static RgbColor[] StandardColours(ReadOnlySpan<byte> palette)
    {
        var colours = new RgbColor[SpriteImage.SpriteColourCount];
        for (int colour = 0; colour < colours.Length; colour++)
            colours[colour] = RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(palette[(colour * 2)..]));
        return colours;
    }

    /// <summary>
    /// Paints tile word <paramref name="word"/> over <paramref name="character"/>'s 32 bytes with its top-left pixel at
    /// (<paramref name="left"/>, <paramref name="top"/>), and returns the rows it was coloured from.
    /// </summary>
    internal SpritePaletteRows PaintTile(int left, int top, SnesBgTilemapWord word, ReadOnlySpan<byte> character, RgbColor[] colours)
    {
        Ensure.LengthEqual(character, CharacterBytes);
        SpritePaletteRows rows = SpritePaletteRows.None;
        for (int y = 0; y < CharacterPixels; y++)
        {
            int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
            for (int x = 0; x < CharacterPixels; x++)
            {
                int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                int colour = (character[row * 2] >> bit & 1) | (character[row * 2 + 1] >> bit & 1) << 1
                    | (character[16 + row * 2] >> bit & 1) << 2 | (character[17 + row * 2] >> bit & 1) << 3;
                if (colour == 0)
                    continue;
                int pixel = (top + y) * Width + left + x;
                int index = word.PaletteIndex * RowColours + colour;
                PaletteIndices[pixel] = (byte)index;
                Pixels[pixel] = colours[index].ToRgbaPixel();
                Priorities[pixel] = word.HasPriority ? (byte)1 : (byte)0;
                PartRows[pixel] = (byte)(word.PaletteIndex | SpriteImage.FixedPartRow);
                rows |= SpritePaletteRowSet.Rows[word.PaletteIndex];
            }
        }
        return rows;
    }

    /// <summary>
    /// Paints two-bit tile word <paramref name="word"/> over <paramref name="character"/>'s 16 bytes with its top-left pixel
    /// at (<paramref name="left"/>, <paramref name="top"/>), keeping each pixel's row times four plus its colour, colour
    /// zero included, as BG3's sheets do; returns the rows it was coloured from.
    /// </summary>
    internal SpritePaletteRows PaintTwoBppTile(int left, int top, SnesBgTilemapWord word, ReadOnlySpan<byte> character, RgbColor[] colours)
    {
        Ensure.LengthEqual(character, TwoBppCharacterBytes);
        SpritePaletteRows rows = SpritePaletteRows.None;
        for (int y = 0; y < CharacterPixels; y++)
        {
            int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
            for (int x = 0; x < CharacterPixels; x++)
            {
                int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                int colour = (character[row * 2] >> bit & 1) | (character[row * 2 + 1] >> bit & 1) << 1;
                int pixel = (top + y) * Width + left + x;
                int index = word.PaletteIndex * TwoBppRowColours + colour;
                PaletteIndices[pixel] = (byte)index;
                Priorities[pixel] = word.HasPriority ? (byte)1 : (byte)0;
                if (colour == 0)
                {
                    Pixels[pixel] = default;
                    PartRows[pixel] = 0;
                    continue;
                }
                Pixels[pixel] = colours[index].ToRgbaPixel();
                PartRows[pixel] = (byte)(index / RowColours | SpriteImage.FixedPartRow);
                rows |= SpritePaletteRowSet.Rows[index / RowColours];
            }
        }
        return rows;
    }

    /// <summary>
    /// Paints one pixel of colour <paramref name="index"/> at <paramref name="at"/>, as a Mode 7 tile
    /// names its colours directly from all 256; colour zero is transparent. Such a pixel has no part row, as no draw
    /// overrides a background's rows.
    /// </summary>
    internal void PaintIndex(Position16<ImagePixels> at, byte index, RgbColor[] colours)
    {
        int pixel = at.Y * Width + at.X;
        PaletteIndices[pixel] = index;
        if (index != 0)
            Pixels[pixel] = colours[index].ToRgbaPixel();
    }

    /// <summary>Colours in one two-bit palette row.</summary>
    private const int TwoBppRowColours = 4;

    /// <summary>Bytes in one two-bit character.</summary>
    internal const int TwoBppCharacterBytes = 16;

    /// <summary>The painted planes as a background sheet coloured as <paramref name="kind"/> with <paramref name="frames"/> cut from it.</summary>
    internal ComposedSpriteSheet ToSheet(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames,
        SpriteImageColours kind = SpriteImageColours.Background) =>
        new(Size, Pixels, PaletteIndices, Priorities, PartRows, new SpriteSheetDocument
        {
            Version = SpriteSheet.Version,
            Palette = kind,
            Colours = [.. colours.Select(colour => colour.ToWord())],
            Frames = frames,
            PartImages = [],
        });
}
