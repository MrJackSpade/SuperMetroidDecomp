using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable room BG tile references, preserving each native 32x32 page's word order.</summary>
public sealed class RoomBackgroundTilemapAtlas
{
    private readonly byte[] transfer;
    private RoomBackgroundTilemapAtlas(byte[] transfer) => this.transfer = transfer;

    /// <summary>Owned compiled tilemap bytes: complete little-endian SNES BG words in page order, with row-major cells inside each 32-by-32 page.</summary>
    public ReadOnlyMemory<byte> Transfer => transfer;

    /// <summary>Compiles an editable tile-reference document into one or two native BG tilemap pages.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <param name="expectedByteCount">Required compiled transfer length: $0800 for one page or $1000 for two.</param>
    /// <returns>An atlas owning newly encoded words, with each cell's character index, palette, priority, and flips preserved.</returns>
    /// <remarks>Page order is retained without inferring horizontal or vertical placement; the consuming background/PPU configuration determines that layout. Parsing accepts property-name casing differences but rejects unknown and duplicate properties.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The requested size, JSON schema, page/cell counts, or character/palette selectors are invalid.</exception>
    public static RoomBackgroundTilemapAtlas Load(Stream json, int expectedByteCount)
    {
        ArgumentNullException.ThrowIfNull(json);
        int pageCount = RoomBackgroundTilemapFormat.ValidatePageCount(expectedByteCount);
        RoomBackgroundTilemapDocument document = JsonAssetDocument.Read<RoomBackgroundTilemapDocument>(
            json, JsonOptions, "room background tilemap");
        if (document.Version != RoomBackgroundTilemapFormat.Version ||
            document.Pages is null || document.Pages.Length != pageCount)
            throw new InvalidDataException(
                $"Room background tilemap requires version {RoomBackgroundTilemapFormat.Version} " +
                $"and {pageCount} ordered 32x32 pages.");

        var bytes = new byte[expectedByteCount];
        for (int page = 0; page < pageCount; page++)
        {
            RoomBackgroundTilemapCell[]? cells = document.Pages[page]?.Cells;
            if (cells is null || cells.Length != RoomBackgroundTilemapFormat.CellsPerPage)
                throw new InvalidDataException(
                    $"Room background page {page} must contain exactly " +
                    $"{RoomBackgroundTilemapFormat.CellsPerPage} ordered cells.");
            for (int cell = 0; cell < cells.Length; cell++)
            {
                RoomBackgroundTilemapCell? definition = cells[cell];
                if (definition is null ||
                    (uint)definition.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                    (uint)definition.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                    (uint)definition.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                    throw new InvalidDataException(
                        $"Room background page {page} cell {cell} has an invalid tile or palette.");
                SnesTileFlipFlags flips =
                    (definition.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (definition.FlipY ? SnesTileFlipFlags.Vertical : 0);
                ushort word = SnesBgTilemapWord.Create(
                    definition.TileRow * RoomBackgroundTilemapFormat.TileColumns +
                    definition.TileColumn, definition.Palette, definition.Priority, flips).Raw;
                BinaryPrimitives.WriteUInt16LittleEndian(
                    bytes.AsSpan((page * RoomBackgroundTilemapFormat.CellsPerPage + cell) * 2), word);
            }
        }
        return new RoomBackgroundTilemapAtlas(bytes);
    }

    /// <summary>Serializes a tilemap document, validating the serialized form against its required native transfer size before writing to the destination.</summary>
    /// <param name="json">Caller-owned writable destination stream, written at its current position without being disposed.</param>
    /// <param name="document">The ordered one- or two-page editable map definition.</param>
    /// <param name="expectedByteCount">Required compiled length, $0800 or $1000 bytes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The document is null or fails the schema, size, cell, or selector checks performed by <see cref="Load"/>.</exception>
    public static void Write(Stream json, RoomBackgroundTilemapDocument document, int expectedByteCount)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), expectedByteCount);
        json.Write(bytes);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Editable schema for one or two ordered native BG tilemap pages; contains visual references rather than room collision or metatile data.</summary>
public sealed record RoomBackgroundTilemapDocument
{
    /// <summary>Schema revision; compilation requires <see cref="RoomBackgroundTilemapFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable array of one or two pages in native transfer order; page arrangement is determined by the consumer.</summary>
    public required RoomBackgroundTilemapPage[] Pages { get; init; }
}

/// <summary>One 32-by-32 BG page containing exactly 1024 ordered tile-reference cells.</summary>
public sealed record RoomBackgroundTilemapPage
{
    /// <summary>Caller-owned mutable cell array in row-major destination order: index <c>y * 32 + x</c> selects a page cell, not a character-sheet position.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

/// <summary>Decoded fields of one sixteen-bit SNES BG tilemap word, separating the character-sheet selector from its destination cell.</summary>
public sealed record RoomBackgroundTilemapCell
{
    /// <summary>Zero-based character-sheet column, 0-31; together with <see cref="TileRow"/> forms character index <c>TileRow * 32 + TileColumn</c>, not the destination map column.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based character-sheet row, 0-31, contributing to the ten-bit character index; not the destination map row.</summary>
    public required int TileRow { get; init; }
    /// <summary>Three-bit BG palette selector, 0-7, encoded in tilemap word bits 10-12; actual colors come from the consuming layer's palette.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether native BG priority bit 13 is set; final layer ordering still depends on the selected PPU mode.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether native bit 14 mirrors the selected character horizontally when rendered.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether native bit 15 mirrors the selected character vertically when rendered.</summary>
    public required bool FlipY { get; init; }
}

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
