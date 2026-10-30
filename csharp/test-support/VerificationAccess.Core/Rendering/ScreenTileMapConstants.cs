using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ScreenTileMap"/> that only verification reads.</summary>
internal static class ScreenTileMapConstants
{
    /// <summary>
    /// Reads a <paramref name="size"/> map of cells from the tilemap at <paramref name="tilemapWord"/> in the screen's
    /// memory image <paramref name="image"/>, each showing one of <paramref name="tiles"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    public static ScreenTileMap Capture(ReadOnlySpan<byte> image, int tilemapWord, Extent16<TilemapCells> size, ScreenLayerPictures tiles)
    {
        Ensure.LengthEqual(image, SnesVram.ByteCount);
        var geometry = new BgTilemapGeometry(size);
        var cells = ImmutableArray.CreateBuilder<BackgroundTile>(size.X * size.Y);
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int offset = SnesVramAddressingConstants.ByteOffsetOfWord(geometry.EntryWordAddress(VramWordRange.Require(tilemapWord),
                    new Position16<TilemapCells>((ushort)x, (ushort)y)));
                var word = new SnesBgTilemapWord(LittleEndianBytes.Word(image[offset], image[offset + 1]));
                cells.Add(new BackgroundTile(word.CharacterIndex, word.PaletteIndex, word.FlipHorizontally, word.FlipVertically, word.HasPriority));
            }
        }
        return new ScreenTileMap(tiles, size, cells.MoveToImmutable());
    }
    /// <summary>Cells along each side of one screen of a tilemap; a larger map keeps whole screens one after another.</summary>
    internal const int ScreenCells = 32;
    /// <summary>
    /// Reads a <paramref name="size"/> map of cells from a screen's own tilemap <paramref name="tilemap"/>, each showing one
    /// of <paramref name="tiles"/>. A map wider or taller than one 32-cell screen keeps its screens one after another,
    /// left to right, then top to bottom.
    /// </summary>
    /// <exception cref="ArgumentException">The tilemap does not hold every cell of the map.</exception>
    public static ScreenTileMap Of(ReadOnlySpan<BackgroundTile> tilemap, Extent16<TilemapCells> size, ScreenLayerPictures tiles)
    {
        Ensure.LengthEqual(tilemap, size.X * size.Y);
        if (size.X % ScreenCells != 0 || size.Y % ScreenCells != 0)
            throw new ArgumentException($"A {size.X}x{size.Y} map is not whole {ScreenCells}-cell screens.", nameof(size));
        int screensAcross = size.X / ScreenCells;
        var cells = ImmutableArray.CreateBuilder<BackgroundTile>(size.X * size.Y);
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int screen = y / ScreenCells * screensAcross + x / ScreenCells;
                cells.Add(tilemap[screen * ScreenCells * ScreenCells + y % ScreenCells * ScreenCells + x % ScreenCells]);
            }
        }
        return new ScreenTileMap(tiles, size, cells.MoveToImmutable());
    }
}
