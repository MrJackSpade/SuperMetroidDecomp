using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable room BG tile references, preserving each native 32x32 page's word order.</summary>
public sealed class RoomBackgroundTilemapAtlas
{
    /// <summary>Owned little-endian BG tile words in native page and row order.</summary>
    private readonly byte[] transfer;
    /// <summary>Retains a validated one- or two-page native transfer independent of JSON arrays.</summary>
    internal RoomBackgroundTilemapAtlas(byte[] transfer) => this.transfer = transfer;

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
        Ensure.NotNull(json);
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
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), expectedByteCount);
        json.Write(bytes);
    }

    /// <summary>Accepts casing variations but rejects unknown or duplicate tilemap properties.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
