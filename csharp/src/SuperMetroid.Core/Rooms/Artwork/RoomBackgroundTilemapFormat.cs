namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Editable tile-reference schema limits and source-keyed filenames for native library-background maps.</summary>
public static class RoomBackgroundTilemapFormat
{
    /// <summary>Required tilemap document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Character-sheet columns used to split a ten-bit native character index into editable column/row selectors.</summary>
    public const int TileColumns = 32;
    /// <summary>Character-sheet rows exposed by the schema; with 32 columns covers all 1024 native character selectors.</summary>
    public const int TileRows = 32;
    /// <summary>Number of distinct values in the native BG word's three-bit palette selector.</summary>
    public const int PaletteCount = 8;
    /// <summary>Number of destination cells in one native 32-by-32 BG tilemap page.</summary>
    public const int CellsPerPage = 32 * 32;
    /// <summary>Compiled byte length of one page, $0800, with one little-endian sixteen-bit word per cell.</summary>
    public const int BytesPerPage = CellsPerPage * sizeof(ushort);
    /// <summary>Number of distinct compressed library-background source identities required by the supported retail cartridge's complete room-background catalog.</summary>
    public const int RetailCompressedSourceCount = 58;

    /// <summary>Creates the JSON filename keyed by a background's immutable compressed cartridge source, allowing shared sources to share one resource.</summary>
    /// <param name="sourceAddress">Full SNES source address, conventionally a 24-bit bank/offset value; this naming method does not validate address mapping.</param>
    /// <returns>The <c>room-background-</c> filename with an uppercase hexadecimal address padded to at least six digits and a <c>.json</c> suffix.</returns>
    public static string SourceFileName(int sourceAddress) =>
        $"room-background-{sourceAddress:X6}.json";

    /// <summary>Checks a native transfer extent and converts its byte length to the supported page count.</summary>
    /// <param name="byteCount">Compiled BG word bytes; only $0800 and $1000 are accepted.</param>
    /// <returns>One or two complete 32-by-32 pages.</returns>
    /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not exactly one or two native page lengths.</exception>
    public static int ValidatePageCount(int byteCount)
    {
        if (byteCount is not BytesPerPage and not (2 * BytesPerPage))
            throw new InvalidDataException(
                $"Room background tilemap must contain one or two native $0800-byte pages, got ${byteCount:X} bytes.");
        return byteCount / BytesPerPage;
    }
}
