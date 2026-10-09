namespace SuperMetroid.Core.Assets;

/// <summary>Editable 2-bpp font used by the English opening narration.</summary>
public sealed class IntroFontAtlas
{
    /// <summary>Holds noncanonical encoded bytes; null selects the compiled canonical glyph definition.</summary>
    private readonly byte[]? suppliedTransfer;

    /// <summary>Stores an edited transfer only when its bytes differ from the canonical opening-font encoding.</summary>
    /// <param name="transfer">The SNES two-bit planar bytes produced from the indexed glyph atlas.</param>
    private IntroFontAtlas(byte[] transfer) => suppliedTransfer =
        transfer.AsSpan().SequenceEqual(IntroFontGlyphDefinitions.Compile()) ? null : transfer;

    /// <summary>Gets the 2,304-byte SNES two-bit planar transfer for all 144 narration glyph tiles.</summary>
    public ReadOnlyMemory<byte> Transfer => suppliedTransfer ?? IntroFontGlyphDefinitions.Compile();

    /// <summary>Loads the indexed opening-font PNG and compiles it to native two-bit planar tiles.</summary>
    /// <param name="png">Caller-owned indexed PNG stream.</param>
    /// <returns>The compiled opening narration font.</returns>
    public static IntroFontAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, IntroFontAtlasFormat.Width, IntroFontAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(
            image.Pixels, image.Width, image.Height, IntroFontAtlasFormat.BitsPerPixel);
        if (planar.Length != IntroFontAtlasFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Opening font compiled to {planar.Length} bytes; expected " +
                $"{IntroFontAtlasFormat.ByteCount}.");
        }
        return new(planar);
    }
}

/// <summary>PNG and native transfer geometry for the 144-tile opening font.</summary>
public static class IntroFontAtlasFormat
{
    /// <summary>Canonical opening-font PNG file name.</summary>
    public const string FileName = "intro-font.png";
    /// <summary>SNES planar bit depth of the opening narration font.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Required number of indexed PNG colors.</summary>
    public const int ColorCount = 4;
    /// <summary>Number of eight-by-eight glyph tiles in the atlas.</summary>
    public const int TileCount = 144;
    /// <summary>Number of glyph tiles in each atlas row.</summary>
    public const int TilesPerRow = 16;
    /// <summary>Width and height of one square glyph tile in pixels.</summary>
    public const int TileSize = 8;
    /// <summary>Required PNG width in pixels.</summary>
    public const int Width = TilesPerRow * TileSize;
    /// <summary>Required PNG height in pixels.</summary>
    public const int Height = TileCount / TilesPerRow * TileSize;
    /// <summary>Byte length of the compiled SNES two-bit planar transfer.</summary>
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
}
