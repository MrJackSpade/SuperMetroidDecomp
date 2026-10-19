using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Samples an immutable background while retaining the last decoded tile row. The tiles come from a
/// <see cref="IBgTileSource"/> (room level data or a VRAM tilemap) and their characters from VRAM. Windowed
/// composition may skip pixels; the cache key is the tile and its row, not call count, so clipping, flipped tiles and
/// discontinuous HDMA remain exact. Create a new sampler after changing VRAM or palette contents.
/// </summary>
[PpuLayoutBoundary("backend: software PPU per-pixel background character fetch")]
internal sealed class SnesBackgroundPixelSampler(SnesVram vram, Rgba32[] colors,
    IBgTileSource tiles, int characters, bool fourBit)
{
    /// <summary>Tile word and source row of the cached decode; minus one forces the first fetch.</summary>
    private int cachedKey = -1;
    /// <summary>Two or four fetched bitplane bytes for the cached source row, reused across its eight pixels.</summary>
    private int plane0, plane1, plane2, plane3;

    /// <summary>Samples the plane pixel at a scrolled position; transparent where the background shows nothing.</summary>
    internal (Rgba32 Color, bool High) Sample(Position16<BgPlanePixels> pixel, bool opaqueZero = false)
    {
        if (!tiles.TrySample(pixel, out BgTileSample sample))
            return (default, false);
        SnesBgTilemapWord tile = sample.Tile;
        int py = tile.FlipVertically ? 7 - sample.Pixel.Y : sample.Pixel.Y;
        int key = tile.Raw << 3 | py;
        if (key != cachedKey)
        {
            int row = ((characters + tile.CharacterIndex * (fourBit ? 16 : 8)) * 2 + py * 2) & (SnesVram.ByteCount - 1);
            plane0 = vram.ReadByte(row);
            plane1 = vram.ReadByte((row + 1) & (SnesVram.ByteCount - 1));
            plane2 = fourBit ? vram.ReadByte((row + 16) & (SnesVram.ByteCount - 1)) : 0;
            plane3 = fourBit ? vram.ReadByte((row + 17) & (SnesVram.ByteCount - 1)) : 0;
            cachedKey = key;
        }
        int shift = tile.FlipHorizontally ? sample.Pixel.X : 7 - sample.Pixel.X;
        int color = (plane0 >> shift & 1) | (plane1 >> shift & 1) << 1 |
            (plane2 >> shift & 1) << 2 | (plane3 >> shift & 1) << 3;
        return (color == 0 && !opaqueZero ? default : colors[tile.PaletteIndex * (fourBit ? 16 : 4) + color], tile.HasPriority);
    }
}
