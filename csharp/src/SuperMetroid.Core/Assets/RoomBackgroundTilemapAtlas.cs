using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable room BG tile references, preserving each native 32x32 page's word order.</summary>
public sealed class RoomBackgroundTilemapAtlas
{
    private readonly byte[] transfer;
    private RoomBackgroundTilemapAtlas(byte[] transfer) => this.transfer = transfer;

    public ReadOnlyMemory<byte> Transfer => transfer;

    public static RoomBackgroundTilemapAtlas Load(Stream json, int expectedByteCount)
    {
        ArgumentNullException.ThrowIfNull(json);
        int pageCount = RoomBackgroundTilemapFormat.ValidatePageCount(expectedByteCount);
        RoomBackgroundTilemapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(json, JsonOptions)
                ?? throw new InvalidDataException("Room background tilemap JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room background tilemap JSON.", error);
        }
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
        WriteIndented = true,
    };
}

public sealed record RoomBackgroundTilemapDocument
{
    public required int Version { get; init; }
    public required RoomBackgroundTilemapPage[] Pages { get; init; }
}

public sealed record RoomBackgroundTilemapPage
{
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

public sealed record RoomBackgroundTilemapCell
{
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required bool Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}

public static class RoomBackgroundTilemapFormat
{
    public const int Version = 1;
    public const int TileColumns = 32;
    public const int TileRows = 32;
    public const int PaletteCount = 8;
    public const int CellsPerPage = 32 * 32;
    public const int BytesPerPage = CellsPerPage * sizeof(ushort);
    public const int RetailCompressedSourceCount = 58;

    public static string SourceFileName(int sourceAddress) =>
        $"room-background-{sourceAddress:X6}.json";

    public static int ValidatePageCount(int byteCount)
    {
        if (byteCount != BytesPerPage && byteCount != 2 * BytesPerPage)
            throw new InvalidDataException(
                $"Room background tilemap must contain one or two native $0800-byte pages, got ${byteCount:X} bytes.");
        return byteCount / BytesPerPage;
    }
}
