using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Samples an immutable background while retaining the last decoded tile row.
/// Windowed composition may skip pixels; the cache key uses source coordinates,
/// not call count, so clipping, flipped tiles and discontinuous HDMA remain exact.
/// Create a new sampler after changing VRAM or palette contents.
/// </summary>
internal sealed class SnesBackgroundPixelSampler(SnesVram vram, Rgba32[] colors,
    int map, int characters, int mapWidth, int mapHeight, bool fourBit)
{
    private int cachedKey = -1;
    private SnesBgTilemapWord tile;
    private int plane0, plane1, plane2, plane3;

    internal (Rgba32 Color, bool High) Sample(int x, int y, bool opaqueZero = false)
    {
        x &= mapWidth * 8 - 1;
        y &= mapHeight * 8 - 1;
        int tx = x >> 3;
        int key = y * mapWidth + tx;
        if (key != cachedKey)
        {
            int ty = y >> 3;
            int page = ((ty >> 5) * (mapWidth >> 5) + (tx >> 5)) * 1024;
            tile = vram.ReadWord((map + page + (ty & 31) * 32 + (tx & 31)) & (SnesVram.WordCount - 1));
            int py = tile.FlipVertically ? 7 - (y & 7) : y & 7;
            int row = ((characters + tile.CharacterIndex * (fourBit ? 16 : 8)) * 2 + py * 2) & (SnesVram.ByteCount - 1);
            plane0 = vram.ReadByte(row);
            plane1 = vram.ReadByte((row + 1) & (SnesVram.ByteCount - 1));
            plane2 = fourBit ? vram.ReadByte((row + 16) & (SnesVram.ByteCount - 1)) : 0;
            plane3 = fourBit ? vram.ReadByte((row + 17) & (SnesVram.ByteCount - 1)) : 0;
            cachedKey = key;
        }
        int shift = tile.FlipHorizontally ? x & 7 : 7 - (x & 7);
        int color = (plane0 >> shift & 1) | (plane1 >> shift & 1) << 1 |
            (plane2 >> shift & 1) << 2 | (plane3 >> shift & 1) << 3;
        return (color == 0 && !opaqueZero ? default : colors[tile.PaletteIndex * (fourBit ? 16 : 4) + color], tile.HasPriority);
    }
}
