using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The installed opening-narration font as the import pipeline reads it (#1409): the 144 glyph tiles compiled to SNES
/// two-bit planar characters, which the intro font tile sheet composes from.
/// </summary>
public sealed class IntroFontCharacters
{
    /// <summary>Canonical opening-font PNG file name.</summary>
    public const string FileName = "intro-font.png";

    /// <summary>Required number of indexed PNG colors.</summary>
    public const int ColorCount = 4;

    /// <summary>Number of glyph tiles in each atlas row.</summary>
    public const int TilesPerRow = 16;

    /// <summary>Required PNG width in pixels.</summary>
    public const int Width = TilesPerRow * IntroFontAtlasFormat.TileSize;

    /// <summary>Required PNG height in pixels.</summary>
    public const int Height = IntroFontAtlasFormat.TileCount / TilesPerRow * IntroFontAtlasFormat.TileSize;

    /// <summary>Installed planar bytes only when the supplied font differs from the compiled native glyphs.</summary>
    internal readonly byte[]? suppliedCharacters;

    /// <summary>Stores edited font bytes while allowing an exact native match to use the compiled glyph source.</summary>
    /// <param name="characters">The font's planar characters.</param>
    internal IntroFontCharacters(byte[] characters) =>
        suppliedCharacters = characters.SequenceEqual(IntroFontGlyphDefinitions.Compile()) ? null : characters;

    /// <summary>Gets the 2,304-byte SNES two-bit planar characters of all 144 narration glyph tiles.</summary>
    public ReadOnlyMemory<byte> Characters => suppliedCharacters ?? IntroFontGlyphDefinitions.Compile();

    /// <summary>Loads the indexed opening-font PNG and compiles it to native two-bit planar tiles.</summary>
    /// <param name="png">Caller-owned indexed PNG stream.</param>
    /// <returns>The compiled opening narration font.</returns>
    /// <exception cref="InvalidDataException">The PNG is malformed, the wrong size, or compiles to the wrong length.</exception>
    public static IntroFontCharacters Read(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, new(IntroFontCharacters.Width, IntroFontCharacters.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(
            image.Pixels, image.Size, IntroFontAtlasFormat.BitsPerPixel);
        if (planar.Length != IntroFontAtlasFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Opening font compiled to {planar.Length} bytes; expected " +
                $"{IntroFontAtlasFormat.ByteCount}.");
        }
        return new(planar);
    }
}
