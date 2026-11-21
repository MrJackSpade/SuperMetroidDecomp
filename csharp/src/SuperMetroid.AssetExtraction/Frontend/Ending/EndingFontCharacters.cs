using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The installed ending font as the import pipeline reads it (#1409): its selected glyph pixels, compiled to SNES 4-bpp
/// characters for the ending's sheets and kept as installed.
/// </summary>
public sealed class EndingFontCharacters
{
    /// <summary>The installed font's 4-bpp characters.</summary>
    private readonly byte[] characters;

    /// <summary>Retains the installed font's 4-bpp characters.</summary>
    /// <param name="characters">The $1400-byte transfer of all 160 font and subtitle tiles in atlas order.</param>
    internal EndingFontCharacters(byte[] characters) => this.characters = characters;

    /// <summary>The $1400-byte SNES 4-bpp transfer containing all 160 selected font/subtitle tiles in atlas order.</summary>
    public ReadOnlyMemory<byte> Characters => characters;

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
        return new(planar);
    }
}
