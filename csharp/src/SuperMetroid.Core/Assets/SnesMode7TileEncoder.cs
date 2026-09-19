namespace SuperMetroid.Core.Assets;

/// <summary>Compiles an indexed tile atlas into Mode 7's chunky eight-bit character order.</summary>
public static class SnesMode7TileEncoder
{
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
