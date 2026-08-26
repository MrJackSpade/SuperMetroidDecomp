namespace SuperMetroid.Core.Assets;

public readonly record struct Rgba32(byte R, byte G, byte B, byte A = 255);

/// <summary>Decoders for the native color and tile formats consumed by the SNES PPU.</summary>
public static class SnesGraphics
{
    /// <summary>
    /// Converts little-endian SNES BGR555 colors into 8-bit RGBA. The console stores five
    /// bits per channel in the order RRRRR, GGGGG, BBBBB from least to most significant.
    /// </summary>
    public static IReadOnlyList<Rgba32> DecodeBgr555Palette(ReadOnlySpan<byte> bytes)
    {
        var colors = new Rgba32[bytes.Length / 2];
        for (int i = 0; i < colors.Length; i++)
        {
            int value = bytes[i * 2] | (bytes[i * 2 + 1] << 8);
            colors[i] = DecodeBgr555Color((ushort)value);
        }
        return colors;
    }

    /// <summary>Converts one native 15-bit CGRAM word into host RGBA.</summary>
    public static Rgba32 DecodeBgr555Color(ushort value)
    {
        // Despite the conventional name "BGR555", the little-endian numeric word places
        // red in bits 0-4, green in 5-9, and blue in 10-14. Bit 15 is not a color bit.
        return new Rgba32(Expand5(value), Expand5(value >> 5), Expand5(value >> 10));
    }

    /// <summary>
    /// Decodes consecutive 8x8 SNES planar tiles into one byte-per-pixel palette indexes.
    /// Two-bit tiles occupy 16 bytes; four-bit tiles occupy 32 bytes.
    /// </summary>
    public static byte[] DecodePlanarTiles(ReadOnlySpan<byte> data, int bitsPerPixel, int tilesPerRow, out int width, out int height)
    {
        if (bitsPerPixel is not (2 or 4))
            throw new ArgumentOutOfRangeException(nameof(bitsPerPixel));

        int bytesPerTile = bitsPerPixel * 8;
        int tileCount = data.Length / bytesPerTile;
        width = Math.Max(1, Math.Min(tilesPerRow, tileCount)) * 8;
        height = Math.Max(1, (tileCount + tilesPerRow - 1) / tilesPerRow) * 8;
        var pixels = new byte[width * height];

        for (int tile = 0; tile < tileCount; tile++)
        {
            int tileX = tile % tilesPerRow * 8;
            int tileY = tile / tilesPerRow * 8;
            int start = tile * bytesPerTile;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // Within each plane byte, the leftmost pixel is the most-significant bit.
                    int mask = 1 << (7 - x);
                    int index = ((data[start + y * 2] & mask) != 0 ? 1 : 0)
                              | ((data[start + y * 2 + 1] & mask) != 0 ? 2 : 0);
                    // Planes 0/1 occupy the first 16 bytes; planes 2/3 occupy the next 16.
                    if (bitsPerPixel == 4)
                    {
                        index |= ((data[start + 16 + y * 2] & mask) != 0 ? 4 : 0)
                               | ((data[start + 16 + y * 2 + 1] & mask) != 0 ? 8 : 0);
                    }
                    pixels[(tileY + y) * width + tileX + x] = (byte)index;
                }
            }
        }
        return pixels;
    }

    /// <summary>
    /// Decodes Mode 7 tiles. Unlike ordinary planar tiles, each Mode 7 pixel is already
    /// stored as one chunky eight-bit palette index, so rows can be copied directly.
    /// </summary>
    public static byte[] DecodeMode7Tiles(ReadOnlySpan<byte> data, int tilesPerRow, out int width, out int height)
    {
        int tileCount = data.Length / 64;
        width = Math.Max(1, Math.Min(tilesPerRow, tileCount)) * 8;
        height = Math.Max(1, (tileCount + tilesPerRow - 1) / tilesPerRow) * 8;
        var pixels = new byte[width * height];
        for (int tile = 0; tile < tileCount; tile++)
        {
            int tileX = tile % tilesPerRow * 8;
            int tileY = tile / tilesPerRow * 8;
            for (int y = 0; y < 8; y++)
                data.Slice(tile * 64 + y * 8, 8).CopyTo(pixels.AsSpan((tileY + y) * width + tileX, 8));
        }
        return pixels;
    }

    /// <summary>
    /// Creates a grayscale palette for inspecting raw tile indexes when the tilemap has
    /// not yet supplied the real palette selection. It is diagnostic, not game artwork.
    /// </summary>
    public static IReadOnlyList<Rgba32> DiagnosticPalette(int colorCount)
    {
        var colors = new Rgba32[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            byte value = colorCount <= 1 ? (byte)0 : (byte)(i * 255 / (colorCount - 1));
            colors[i] = new Rgba32(value, value, value);
        }
        return colors;
    }

    // Replicate the high three bits into the low end rather than merely shifting. This maps
    // SNES 0..31 exactly onto the full 0..255 display range, including both endpoints.
    private static byte Expand5(int value) => (byte)(((value & 31) << 3) | ((value & 31) >> 2));
}
