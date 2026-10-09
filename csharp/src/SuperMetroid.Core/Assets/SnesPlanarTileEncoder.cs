namespace SuperMetroid.Core.Assets;

/// <summary>Compiles native-size indexed artwork into the PPU's 2/4-bpp character format, not a CPU memory image.</summary>
public static class SnesPlanarTileEncoder
{
    /// <summary>Compiles row-major palette indices into consecutive SNES characters in left-to-right, top-to-bottom tile order; each bitplane pair interleaves its eight row bytes, with the leftmost pixel in bit 7.</summary>
    /// <param name="pixels">Exactly <paramref name="width"/> times <paramref name="height"/> indices, each 0..3 for two-bit encoding or 0..15 for four-bit encoding; no RGB conversion or index remapping is performed.</param>
    /// <param name="width">Positive image width in pixels, divisible by eight.</param>
    /// <param name="height">Positive image height in pixels, divisible by eight.</param>
    /// <param name="bitsPerPixel">Supported native bitplane count: two or four, producing 16 or 32 bytes per 8-by-8 character.</param>
    /// <returns>A newly allocated planar character payload with no palette, tilemap, or VRAM destination metadata.</returns>
    /// <exception cref="ArgumentException">The bitplane count, complete-tile dimensions, or pixel count is invalid.</exception>
    /// <exception cref="InvalidDataException">A pixel index cannot be represented by the selected bitplane count.</exception>
    /// <exception cref="OverflowException">The dimension product exceeds the signed 32-bit range.</exception>
    public static byte[] Encode(ReadOnlySpan<byte> pixels, int width, int height, int bitsPerPixel)
    {
        if (bitsPerPixel is not (2 or 4) || width <= 0 || height <= 0 || width % 8 != 0 || height % 8 != 0 ||
            pixels.Length != checked(width * height)) throw new ArgumentException("Planar artwork requires complete 8x8 tiles at 2 or 4 bpp.");
        var bytes = new byte[pixels.Length * bitsPerPixel / 8];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            byte pixel = pixels[y * width + x];
            if (pixel >= 1 << bitsPerPixel) throw new InvalidDataException($"Atlas pixel ({x},{y}) exceeds its {1 << bitsPerPixel}-color index range.");
            int tile = y / 8 * (width / 8) + x / 8;
            for (int plane = 0; plane < bitsPerPixel; plane++)
                bytes[tile * bitsPerPixel * 8 + plane / 2 * 16 + y % 8 * 2 + plane % 2] |=
                    (byte)(((pixel >> plane) & 1) << (7 - x % 8));
        }
        return bytes;
    }
}
