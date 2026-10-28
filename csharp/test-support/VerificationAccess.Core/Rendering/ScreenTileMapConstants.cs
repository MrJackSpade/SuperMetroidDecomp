using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ScreenTileMap"/> that only verification reads.</summary>
internal static class ScreenTileMapConstants
{
    /// <summary>
    /// Reads a <paramref name="size"/> map of cells from the tilemap at <paramref name="tilemapWord"/> in the screen's
    /// memory image <paramref name="image"/>, each showing one of <paramref name="tiles"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    [PpuLayoutBoundary("capture: a frontend layer's tilemap words, read from the screen's memory image into cells")]
    public static ScreenTileMap Capture(ReadOnlySpan<byte> image, int tilemapWord, Extent16<TilemapCells> size, ScreenLayerPictures tiles)
    {
        Ensure.LengthEqual(image, SnesPpuLayout.VramByteCount);
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
}
