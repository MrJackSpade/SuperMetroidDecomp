namespace SuperMetroid.Core.Assets;

/// <summary>Editable 4-bpp font shared by the credits, result screens, and post-shot subtitle.</summary>
public sealed class EndingFontAtlas
{
    // Footprint positions, native cutouts and all other artwork remain required.
    private readonly HashSet<int> glyphFootprint = new();
    private readonly Dictionary<int, byte> pixels = new();
    private EndingFontAtlas(byte[] supplied)
    {
        for (int pixel = 0; pixel < supplied.Length; pixel++)
            if (EndingFontAtlasFormat.IsOutlinedAlphabetPixel(pixel) && supplied[pixel] == EndingFontAtlasFormat.GlyphFillInk)
                glyphFootprint.Add(pixel);
        for (int pixel = 0; pixel < supplied.Length; pixel++)
            if (!EndingFontAtlasFormat.TryCalculatedPixel(pixel, glyphFootprint, out byte calculated)
                || supplied[pixel] != calculated) pixels[pixel] = supplied[pixel];
    }
    public ReadOnlyMemory<byte> Transfer => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, EndingFontAtlasFormat.Width * EndingFontAtlasFormat.Height)
            .Select(Pixel).ToArray(),
        EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height, EndingFontAtlasFormat.BitsPerPixel);

    private byte Pixel(int pixel) => pixels.TryGetValue(pixel, out byte value) ? value
        : EndingFontAtlasFormat.TryCalculatedPixel(pixel, glyphFootprint, out byte calculated) ? calculated
        : throw new InvalidOperationException("Required ending font pixel is absent.");
    public static EndingFontAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(
            image.Pixels, image.Width, image.Height, EndingFontAtlasFormat.BitsPerPixel);
        if (planar.Length != EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font compiled to {planar.Length} bytes; expected {EndingFontAtlasFormat.ByteCount}.");
        return new(image.Pixels);
    }

    /// <summary>Constructs the fallback cartridge-backed atlas used by focused legacy ending tests.</summary>
    internal static EndingFontAtlas FromPlanarBytes(ReadOnlySpan<byte> planar)
    {
        if (planar.Length != EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font contains {planar.Length} planar bytes; expected {EndingFontAtlasFormat.ByteCount}.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, EndingFontAtlasFormat.BitsPerPixel,
            EndingFontAtlasFormat.TilesPerRow, out _, out _);
        return new(pixels);
    }
}

/// <summary>PNG and native transfer geometry for the 160-tile ending font resource.</summary>
public static class EndingFontAtlasFormat
{
    public const string FileName = "ending-font.png";
    public const int BitsPerPixel = 4;
    public const int ColorCount = 16;
    public const int TileCount = 160;
    public const int TilesPerRow = 16;
    public const int TileSize = 8;
    public const int Width = TilesPerRow * TileSize;
    public const int Height = TileCount / TilesPerRow * TileSize;
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
    /// <summary>
    /// Font3 compressed at $97:E7DE: native result blank tile$4F and large blank tile$7F,
    /// selected as space by the ending text tilemaps/glyph mapping, contain transparent ink.
    /// This operation covers only those two space cells; the outline operation below handles its separately identified glyph domain.
    /// </summary>
    internal static bool IsSpacePixel(int pixel)
    {
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel / Width / TileSize * TilesPerRow + pixel % Width / TileSize;
        return tile is EndingTextDefinitions.ResultBlankWord or EndingTextDefinitions.LargeBlankWord;
    }
    /// <summary>Font3 $97:E7DE, small/large alphabet and copyright digits: REQUIRED selected foreground pen1.</summary>
    internal const byte GlyphFillInk = 1;
    /// <summary>Font3 $97:E7DE, small/large alphabet and copyright digits: REQUIRED selected outline pen2.</summary>
    internal const byte GlyphOutlineInk = 2;
    /// <summary>Font3 $97:E7DE, small/large alphabet and copyright digits: REQUIRED one-pixel eight-neighbor outline extent.</summary>
    private const int GlyphOutlineWidth = 1;

    internal static bool IsSmallAlphabetPixel(int pixel)
    {
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel / Width / TileSize * TilesPerRow + pixel % Width / TileSize;
        return tile >= EndingTextDefinitions.ResultLetterBase
            && tile <= EndingTextDefinitions.ResultLetterBase + 'Z' - 'A';
    }

    /// <summary>
    /// REQUIRED native deviations from the outline rule: K tile0A at(3,0)/(7,4),
    /// M tile0C at(4,1), Z tile19 at(7,3). These exact pixels have no retention
    /// exception; failure to match the outline rule does not establish nonsense.
    /// </summary>
    internal static bool IsRequiredSmallGlyphCutout(int pixel)
    {
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel / Width / TileSize * TilesPerRow + pixel % Width / TileSize;
        int x = pixel % TileSize, y = pixel / Width % TileSize;
        return tile == EndingTextDefinitions.ResultLetterBase + 'K' - 'A' && ((x, y) is (3, 0) or (7, 4))
            || tile == EndingTextDefinitions.ResultLetterBase + 'M' - 'A' && (x, y) is (4, 1)
            || tile == EndingTextDefinitions.ResultLetterBase + 'Z' - 'A' && (x, y) is (7, 3);
    }

    /// <summary>Native small A–Z plus large A–Z and copyright0–9 glyph cells; other symbols/subtitle pixels remain independently required.</summary>
    internal static bool IsOutlinedAlphabetPixel(int pixel) => IsSmallAlphabetPixel(pixel) || TryLargeGlyphOrigin(pixel, out _);

    /// <summary>Font3 large A–P tops20..2F, Q–Z tops40..49 and copyright digit tops60..69; lower halves lie one atlas row later.</summary>
    internal static bool TryLargeGlyphOrigin(int pixel, out int origin)
    {
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel / Width / TileSize * TilesPerRow + pixel % Width / TileSize;
        int topTile = tile - tile / TilesPerRow % EndingTextDefinitions.Native.LargeGlyphHeight * TilesPerRow;
        bool large = topTile >= EndingTextDefinitions.LargeFirstGroupTopBase
                && topTile < EndingTextDefinitions.LargeFirstGroupTopBase + EndingTextDefinitions.LargeSecondGroupFirstLetter
            || topTile >= EndingTextDefinitions.LargeSecondGroupTopBase
                && topTile <= EndingTextDefinitions.LargeSecondGroupTopBase + 'Z' - 'A' - EndingTextDefinitions.LargeSecondGroupFirstLetter
            || topTile >= EndingTextDefinitions.CopyrightDigitTopBase
                && topTile <= EndingTextDefinitions.CopyrightDigitTopBase + '9' - '0';
        origin = large ? topTile / TilesPerRow * TileSize * Width + topTile % TilesPerRow * TileSize : 0;
        return large;
    }

    /// <summary>
    /// REQUIRED31 native outline deviations in Font3 large B/D/G/J/K/Q/R/S/V.
    /// Coordinates are relative to each two-cell glyph; the source values remain
    /// supplied basis pixels, not outputs of this predicate or approved exceptions.
    /// </summary>
    internal static bool IsRequiredLargeGlyphDeviation(int pixel)
    {
        if (!TryLargeGlyphOrigin(pixel, out int origin)) return false;
        int tile = origin / Width / TileSize * TilesPerRow + origin % Width / TileSize;
        int x = (pixel - origin) % Width, y = (pixel - origin) / Width;
        return (tile, x, y) is
            (0x21, 5, 2) or (0x21, 6, 3) or (0x21, 7, 4) or (0x21, 7, 7)
            or (0x21, 7, 8) or (0x21, 7, 13) or (0x21, 6, 14)
            or (0x23, 6, 3) or (0x23, 7, 5) or (0x23, 7, 11) or (0x23, 6, 13) or (0x23, 5, 14)
            or (0x26, 4, 6) or (0x26, 4, 10)
            or (0x29, 0, 13) or (0x29, 7, 13) or (0x29, 1, 14) or (0x29, 6, 14)
            or (0x2A, 5, 2) or (0x2A, 4, 3) or (0x2A, 5, 8) or (0x2A, 6, 9) or (0x2A, 7, 10)
            or (0x40, 7, 3) or (0x41, 7, 10) or (0x41, 7, 11)
            or (0x42, 7, 13) or (0x42, 1, 14) or (0x42, 6, 14)
            or (0x45, 6, 11) or (0x45, 2, 14);
    }
    internal static bool TryCalculatedPixel(int pixel, IReadOnlySet<int> requiredFootprint, out byte ink)
    {
        ink = 0;
        if (IsSpacePixel(pixel)) return true;
        bool large = TryLargeGlyphOrigin(pixel, out int largeOrigin);
        if ((!IsSmallAlphabetPixel(pixel) && !large) || IsRequiredSmallGlyphCutout(pixel)
            || IsRequiredLargeGlyphDeviation(pixel)) return false;
        if (requiredFootprint.Contains(pixel))
        {
            ink = GlyphFillInk;
            return true;
        }
        int x = pixel % TileSize;
        int y = large ? (pixel - largeOrigin) / Width : pixel / Width % TileSize;
        int origin = large ? largeOrigin : pixel - y * Width - x;
        int height = large ? EndingTextDefinitions.Native.LargeGlyphHeight * TileSize : TileSize;
        for (int adjacentY = Math.Max(0, y - GlyphOutlineWidth); adjacentY <= Math.Min(height - 1, y + GlyphOutlineWidth); adjacentY++)
        for (int adjacentX = Math.Max(0, x - GlyphOutlineWidth); adjacentX <= Math.Min(TileSize - 1, x + GlyphOutlineWidth); adjacentX++)
            if (requiredFootprint.Contains(origin + adjacentY * Width + adjacentX))
            {
                ink = GlyphOutlineInk;
                return true;
            }
        return true;
    }
}
