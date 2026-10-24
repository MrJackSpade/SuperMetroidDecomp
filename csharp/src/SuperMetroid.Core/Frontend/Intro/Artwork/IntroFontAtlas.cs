using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable 2-bpp font used by the English opening narration.</summary>
public sealed class IntroFontAtlas
{
    /// <summary>Installed planar bytes only when the supplied font differs from the compiled native glyphs.</summary>
    internal readonly byte[]? suppliedCharacters;

    /// <summary>Stores edited font bytes while allowing an exact native match to use the compiled glyph source.</summary>
    internal IntroFontAtlas(byte[] characters) => suppliedCharacters =
        characters.SequenceEqual(IntroFontGlyphDefinitions.Compile()) ? null : characters;

    /// <summary>Gets the 2,304-byte SNES two-bit planar characters of all 144 narration glyph tiles.</summary>
    public ReadOnlyMemory<byte> Characters => suppliedCharacters ?? IntroFontGlyphDefinitions.Compile();

    /// <summary>Loads the indexed opening-font PNG and compiles it to native two-bit planar tiles.</summary>
    /// <param name="png">Caller-owned indexed PNG stream.</param>
    /// <returns>The compiled opening narration font.</returns>
    public static IntroFontAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, new(IntroFontAtlasFormat.Width, IntroFontAtlasFormat.Height));
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
