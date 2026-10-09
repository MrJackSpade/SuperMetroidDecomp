namespace SuperMetroid.Core.Assets;

/// <summary>Compiles an indexed tile atlas into Mode 7's chunky eight-bit character order.</summary>
public static class SnesMode7TileEncoder
{
    /// <summary>Reorders an indexed image into consecutive 8-by-8 Mode-7 characters, traversing characters and their internal pixels left to right then top to bottom without changing any eight-bit pen value.</summary>
    /// <param name="pixels">Row-major image indices, exactly <paramref name="width"/> times <paramref name="height"/> bytes; all values 0..255 are retained without palette lookup.</param>
    /// <param name="width">Positive image width in pixels, divisible by eight.</param>
    /// <param name="height">Positive image height in pixels, divisible by eight.</param>
    /// <returns>A newly allocated, equally sized character payload with 64 bytes per tile; contains neither tilemap indices nor the alternating VRAM byte-lane padding.</returns>
    /// <exception cref="ArgumentException">Dimensions are not positive complete-tile extents, or the pixel count differs from their product.</exception>
    /// <exception cref="OverflowException">The dimension product exceeds the signed 32-bit range.</exception>
    public static byte[] Encode(ReadOnlySpan<byte> pixels, int width, int height)
    {
        if (width <= 0 || height <= 0 || width % 8 != 0 || height % 8 != 0 ||
            pixels.Length != checked(width * height))
        {
            throw new ArgumentException("Mode 7 artwork requires complete 8x8 tiles.");
        }

        int tilesPerRow = width / 8;
        var bytes = new byte[pixels.Length];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int tile = y / 8 * tilesPerRow + x / 8;
            bytes[tile * 64 + y % 8 * 8 + x % 8] = pixels[y * width + x];
        }
        return bytes;
    }
}
