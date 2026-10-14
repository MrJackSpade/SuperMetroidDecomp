namespace SuperMetroid.Core.Assets;

/// <summary>Editable 4-bpp font shared by the credits, result screens, and post-shot subtitle.</summary>
public sealed class EndingFontAtlas
{
    /// <summary>Selected fill-ink footprint for calculable alphabet and digit outlines.</summary>
    internal readonly HashSet<int> glyphFootprint = new();
    /// <summary>Required native cutouts, other artwork, and supplied differences from calculated ink.</summary>
    internal readonly Dictionary<int, byte> pixels = new();
    /// <summary>Separates calculable glyph ink from independently required or edited pixels.</summary>
    internal EndingFontAtlas(byte[] supplied)
    {
        for (int pixel = 0; pixel < supplied.Length; pixel++)
            if (EndingFontAtlasFormat.IsOutlinedAlphabetPixel(pixel) && supplied[pixel] == EndingFontAtlasFormat.GlyphFillInk)
                glyphFootprint.Add(pixel);
        for (int pixel = 0; pixel < supplied.Length; pixel++)
            if (!EndingFontAtlasFormat.TryCalculatedPixel(pixel, glyphFootprint, out byte calculated)
                || supplied[pixel] != calculated) pixels[pixel] = supplied[pixel];
    }
    /// <summary>Materializes a new $1400-byte SNES 4-bpp transfer containing all 160 selected font/subtitle tiles in atlas order, without retaining a packed-transfer cache.</summary>
    public ReadOnlyMemory<byte> Transfer => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, EndingFontAtlasFormat.Width * EndingFontAtlasFormat.Height)
            .Select(Pixel).ToArray(),
        new(EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height), EndingFontAtlasFormat.BitsPerPixel);

    /// <summary>Resolves one selected atlas pixel from retained artwork or its glyph outline rule.</summary>
    private byte Pixel(int pixel) => pixels.TryGetValue(pixel, out byte value) ? value
        : EndingFontAtlasFormat.TryCalculatedPixel(pixel, glyphFootprint, out byte calculated) ? calculated
        : throw new InvalidOperationException("Required ending font pixel is absent.");
    /// <summary>Loads the 128x80 indexed ending-font sheet and validates its sixteen-color index range and native transfer size, preserving supplied artwork rather than matching PNG RGB colors.</summary>
    /// <param name="png">Noninterlaced indexed PNG stream at its current position, read through the image end and left open.</param>
    /// <returns>Selected font artwork for credits, result text, and the post-shot subtitle; displayed colors remain palette-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="png"/> is null.</exception>
    /// <exception cref="InvalidDataException">The PNG, dimensions, palette indices, or compiled transfer size are invalid.</exception>
    public static EndingFontAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, new(EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(
            image.Pixels, image.Size, EndingFontAtlasFormat.BitsPerPixel);
        if (planar.Length != EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font compiled to {planar.Length} bytes; expected {EndingFontAtlasFormat.ByteCount}.");
        return new(image.Pixels);
    }
}
