namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>PNG and native transfer geometry for the 160-tile ending font resource.</summary>
public static class EndingFontAtlasFormat
{
    /// <summary>Installed indexed-PNG resource, <c>ending-font.png</c>, extracted from the native Font3 source at $97:E7DE.</summary>
    public const string FileName = "ending-font.png";
    /// <summary>Four SNES bit planes per font tile, giving a 32-byte transfer per eight-by-eight character.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>Sixteen permitted palette-index values, 0..15; these are glyph ink indices rather than supplied RGB palette colors.</summary>
    public const int ColorCount = 16;
    /// <summary>160 contiguous selected Font3 characters, including small/large letters, symbols, and post-shot subtitle artwork.</summary>
    public const int TileCount = 160;
    /// <summary>Sixteen eight-pixel tile columns per indexed-PNG row, preserving native character order.</summary>
    public const int TilesPerRow = 16;
    /// <summary>Eight pixels along each side of a native square font character; large glyphs occupy vertically adjacent characters.</summary>
    public const int TileSize = 8;
    /// <summary>Indexed-PNG width of 128 pixels, derived from sixteen eight-pixel character columns.</summary>
    public const int Width = TilesPerRow * TileSize;
    /// <summary>Indexed-PNG height of 80 pixels, derived from ten eight-pixel character rows.</summary>
    public const int Height = TileCount / TilesPerRow * TileSize;
    /// <summary>$1400 bytes (5120) in the complete selected 4-bpp transfer: 160 characters at 32 bytes each.</summary>
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
        foreach (SpaceCell space in SpaceCells)
            if (TileOf(space) == tile) return true;
        return false;
    }

    /// <summary>Two text-mapped space tiles with transparent native ink.</summary>
    private enum SpaceCell
    {
        /// <summary>Small result-text space tile $4F.</summary>
        ResultBlank,
        /// <summary>Large ending-text space tile $7F.</summary>
        LargeBlank,
    }

    /// <summary>Both native space cells used by the result and large text layouts.</summary>
    private static readonly SpaceCell[] SpaceCells = Enum.GetValues<SpaceCell>();

    /// <summary>Maps the semantic space role to its tile word in the ending text layout.</summary>
    private static int TileOf(SpaceCell space) => space switch
    {
        SpaceCell.ResultBlank => EndingTextDefinitions.ResultBlankWord,
        SpaceCell.LargeBlank => EndingTextDefinitions.LargeBlankWord,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpaceCell)} {(int)space}."),
    };
    /// <summary>Font3 $97:E7DE, small/large alphabet and copyright digits: REQUIRED selected foreground pen1.</summary>
    internal const byte GlyphFillInk = 1;
    /// <summary>Font3 $97:E7DE, small/large alphabet and copyright digits: REQUIRED selected outline pen2.</summary>
    internal const byte GlyphOutlineInk = 2;
    /// <summary>Font3 $97:E7DE, small/large alphabet and copyright digits: REQUIRED one-pixel eight-neighbor outline extent.</summary>
    private const int GlyphOutlineWidth = 1;

    /// <summary>Recognizes the small result alphabet's contiguous tile range.</summary>
    internal static bool IsSmallAlphabetPixel(int pixel)
    {
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel / Width / TileSize * TilesPerRow + pixel % Width / TileSize;
        return tile is >= EndingTextDefinitions.ResultLetterBase
            and <= (EndingTextDefinitions.ResultLetterBase + 'Z' - 'A');
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
        bool large = topTile is >= EndingTextDefinitions.LargeFirstGroupTopBase
                and < (EndingTextDefinitions.LargeFirstGroupTopBase + EndingTextDefinitions.LargeSecondGroupFirstLetter)
            or >= EndingTextDefinitions.LargeSecondGroupTopBase
                and <= (EndingTextDefinitions.LargeSecondGroupTopBase + 'Z' - 'A' - EndingTextDefinitions.LargeSecondGroupFirstLetter)
            or >= EndingTextDefinitions.CopyrightDigitTopBase
                and <= (EndingTextDefinitions.CopyrightDigitTopBase + '9' - '0');
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
        return (LargeLetter(tile), x, y) is
            ('B', 5, 2) or ('B', 6, 3) or ('B', 7, 4) or ('B', 7, 7)
            or ('B', 7, 8) or ('B', 7, 13) or ('B', 6, 14)
            or ('D', 6, 3) or ('D', 7, 5) or ('D', 7, 11) or ('D', 6, 13) or ('D', 5, 14)
            or ('G', 4, 6) or ('G', 4, 10)
            or ('J', 0, 13) or ('J', 7, 13) or ('J', 1, 14) or ('J', 6, 14)
            or ('K', 5, 2) or ('K', 4, 3) or ('K', 5, 8) or ('K', 6, 9) or ('K', 7, 10)
            or ('Q', 7, 3) or ('R', 7, 10) or ('R', 7, 11)
            or ('S', 7, 13) or ('S', 1, 14) or ('S', 6, 14)
            or ('V', 6, 11) or ('V', 2, 14);
    }
    /// <summary>The large letter whose glyph's top cell is <paramref name="topTile"/>, or none for a copyright digit.</summary>
    private static char LargeLetter(int topTile) =>
        topTile >= EndingTextDefinitions.CopyrightDigitTopBase ? ' '
        : topTile >= EndingTextDefinitions.LargeSecondGroupTopBase
            ? (char)('A' + EndingTextDefinitions.LargeSecondGroupFirstLetter + topTile - EndingTextDefinitions.LargeSecondGroupTopBase)
            : (char)('A' + topTile - EndingTextDefinitions.LargeFirstGroupTopBase);

    /// <summary>Calculates transparent, fill, or adjacent outline ink only for the proven glyph domain.</summary>
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
