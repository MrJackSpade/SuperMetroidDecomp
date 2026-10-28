using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The installed ending font as the import pipeline reads it (#1409): its selected glyph pixels, held as the calculable
/// glyph footprint and the pixels that differ from it, compiled to SNES 4-bpp characters for the ending's sheets.
/// </summary>
public sealed class EndingFontCharacters : EndingFontAtlas
{
    /// <summary>Selected fill-ink footprint for calculable alphabet and digit outlines.</summary>
    internal readonly HashSet<int> glyphFootprint = new();
    /// <summary>Required native cutouts, other artwork, and supplied differences from calculated ink.</summary>
    internal readonly Dictionary<int, byte> pixels = new();
    /// <summary>Separates calculable glyph ink from independently required or edited pixels.</summary>
    /// <param name="supplied">The font's row-major pixel indices.</param>
    internal EndingFontCharacters(byte[] supplied) : base(supplied.Length / (EndingFontAtlasFormat.TileSize * EndingFontAtlasFormat.TileSize))
    {
        for (int pixel = 0; pixel < supplied.Length; pixel++)
            if (EndingFontAtlasFormat.IsOutlinedAlphabetPixel(pixel) && supplied[pixel] == EndingFontAtlasFormat.GlyphFillInk)
                glyphFootprint.Add(pixel);
        for (int pixel = 0; pixel < supplied.Length; pixel++)
            if (!EndingFontAtlasFormat.TryCalculatedPixel(pixel, glyphFootprint, out int calculated)
                || supplied[pixel] != calculated) pixels[pixel] = supplied[pixel];
    }
    /// <summary>Materializes a new $1400-byte SNES 4-bpp transfer containing all 160 selected font/subtitle tiles in atlas order, without retaining a packed-transfer cache.</summary>
    public ReadOnlyMemory<byte> Characters => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, EndingFontAtlasFormat.Width * EndingFontAtlasFormat.Height)
            .Select(Pixel).ToArray(),
        new(EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height), EndingFontAtlasFormat.BitsPerPixel);

    /// <summary>Resolves one selected atlas pixel from retained artwork or its glyph outline rule.</summary>
    private byte Pixel(int pixel) => pixels.ContainsKey(pixel) ? pixels[pixel]
        : EndingFontAtlasFormat.TryCalculatedPixel(pixel, glyphFootprint, out int calculated) ? (byte)calculated
        : throw new InvalidOperationException("Required ending font pixel is absent.");
    /// <summary>Loads the 128x80 indexed ending-font sheet and validates its sixteen-color index range and native transfer size, preserving supplied artwork rather than matching PNG RGB colors.</summary>
    /// <param name="png">Noninterlaced indexed PNG stream at its current position, read through the image end and left open.</param>
    /// <returns>Selected font artwork for credits, result text, and the post-shot subtitle; displayed colors remain palette-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="png"/> is null.</exception>
    /// <exception cref="InvalidDataException">The PNG, dimensions, palette indices, or compiled transfer size are invalid.</exception>
    public static EndingFontCharacters Read(Stream png)
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

    /// <summary>The characters <paramref name="font"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The font was not read from installed artwork.</exception>
    public static EndingFontCharacters Of(EndingFontAtlas font) => font as EndingFontCharacters
        ?? throw new InvalidDataException("The ending font was not read from installed artwork.");
}
