using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable 16x16 visual blocks. This catalog contains only four BG tile words per
/// block; room collision types, BTS behavior and placement remain elsewhere.
/// </summary>
public sealed class RoomMetatileAtlas
{
    private readonly byte[] blockDefinitions;

    private RoomMetatileAtlas(byte[] blockDefinitions) => this.blockDefinitions = blockDefinitions;

    public ReadOnlyMemory<byte> Transfer => blockDefinitions;

    public static RoomMetatileAtlas Load(Stream json, int expectedNativeByteCount)
    {
        ArgumentNullException.ThrowIfNull(json);
        int expectedCount = RoomMetatileFormat.ValidateBlockCount(expectedNativeByteCount);
        RoomMetatileDocument document;
        try
        {
            document = JsonSerializer.Deserialize<RoomMetatileDocument>(json, JsonOptions)
                ?? throw new InvalidDataException("Room metatile JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room metatile JSON.", error);
        }
        if (document.Version != RoomMetatileFormat.Version || document.Blocks is null ||
            document.Blocks.Length != expectedCount)
            throw new InvalidDataException(
                $"Room metatiles require version {RoomMetatileFormat.Version} and " +
                $"{expectedCount} ordered 16x16 blocks.");

        var bytes = new byte[expectedNativeByteCount];
        for (int block = 0; block < document.Blocks.Length; block++)
        {
            RoomMetatileDefinition? definition = document.Blocks[block];
            if (definition is null)
                throw new InvalidDataException($"Room metatile {block} is null.");
            WriteCell(definition.TopLeft, block, "topLeft", 0);
            WriteCell(definition.TopRight, block, "topRight", 1);
            WriteCell(definition.BottomLeft, block, "bottomLeft", 2);
            WriteCell(definition.BottomRight, block, "bottomRight", 3);
        }
        return new RoomMetatileAtlas(bytes);

        void WriteCell(RoomMetatileCell? cell, int block, string quadrant, int index)
        {
            if (cell is null || (uint)cell.TileColumn >= RoomMetatileFormat.TileColumns ||
                (uint)cell.TileRow >= RoomMetatileFormat.TileRows ||
                (uint)cell.Palette >= RoomMetatileFormat.PaletteCount)
                throw new InvalidDataException(
                    $"Room metatile {block} {quadrant} has an invalid tile or palette index.");
            SnesTileFlipFlags flips =
                (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (cell.FlipY ? SnesTileFlipFlags.Vertical : 0);
            ushort word = SnesBgTilemapWord.Create(
                cell.TileRow * RoomMetatileFormat.TileColumns + cell.TileColumn,
                cell.Palette, cell.Priority, flips).Raw;
            BinaryPrimitives.WriteUInt16LittleEndian(
                bytes.AsSpan(block * RoomMetatileFormat.BytesPerBlock + index * sizeof(ushort)), word);
        }
    }

    public static void Write(Stream json, RoomMetatileDocument document, int expectedNativeByteCount)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), expectedNativeByteCount);
        json.Write(bytes);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}

public sealed record RoomMetatileDocument
{
    public required int Version { get; init; }
    public required RoomMetatileDefinition[] Blocks { get; init; }
}

public sealed record RoomMetatileDefinition
{
    public required RoomMetatileCell TopLeft { get; init; }
    public required RoomMetatileCell TopRight { get; init; }
    public required RoomMetatileCell BottomLeft { get; init; }
    public required RoomMetatileCell BottomRight { get; init; }
}

public sealed record RoomMetatileCell
{
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required bool Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}

public static class RoomMetatileFormat
{
    public const int Version = 1;
    public const int BytesPerBlock = RoomAssetRomData.GraphicsLayout.BytesPerBlockDefinition;
    public const int MaximumBlockCount = 1024;
    public const int TileColumns = 32;
    public const int TileRows = 32;
    public const int PaletteCount = 8;
    public const string CreFileName = "room-blocks-cre.json";

    public static string SourceFileName(int sourceAddress) => $"room-blocks-{sourceAddress:X6}.json";

    public static int ValidateBlockCount(int byteCount)
    {
        if (byteCount <= 0 || byteCount > MaximumBlockCount * BytesPerBlock ||
            byteCount % BytesPerBlock != 0)
            throw new InvalidDataException(
                $"Room metatiles require 1..{MaximumBlockCount} complete eight-byte blocks; got {byteCount} bytes.");
        return byteCount / BytesPerBlock;
    }
}
