using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Editable 16x16 visual blocks. This catalog contains only four BG tile words per
/// block; room collision types, BTS behavior and placement remain elsewhere.
/// </summary>
public sealed class RoomMetatileAtlas
{
    /// <summary>Four packed BG words per block in native quadrant order.</summary>
    private readonly byte[] blockDefinitions;

    /// <summary>Accepts a fully validated native block table.</summary>
    private RoomMetatileAtlas(byte[] blockDefinitions) => this.blockDefinitions = blockDefinitions;

    /// <summary>Compiled native block-table bytes: four little-endian BG tilemap words per block, ordered top-left, top-right, bottom-left, bottom-right.</summary>
    public ReadOnlyMemory<byte> Transfer => blockDefinitions;

    /// <summary>Validates editable metatile JSON and packs each block's four tile references and visual attributes into the native eight-byte representation.</summary>
    /// <param name="json">Map document stream using the strict, camel-case version-1 schema.</param>
    /// <param name="expectedNativeByteCount">Required decompressed table length in bytes, comprising 1..1024 complete eight-byte blocks.</param>
    /// <returns>The compiled block table, preserving the document's block and quadrant order.</returns>
    /// <exception cref="ArgumentNullException">The JSON stream is null.</exception>
    /// <exception cref="InvalidDataException">The table length, JSON schema, block count, quadrant, tile coordinate, or palette index is invalid.</exception>
    public static RoomMetatileAtlas Load(Stream json, int expectedNativeByteCount)
    {
        Ensure.NotNull(json);
        int expectedCount = RoomMetatileFormat.ValidateBlockCount(expectedNativeByteCount);
        RoomMetatileDocument document = JsonAssetDocument.Read<RoomMetatileDocument>(
            json, JsonOptions, "room metatile");
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

    /// <summary>Serializes and recompiles a metatile document to validate it before writing any JSON bytes to the destination.</summary>
    /// <param name="json">Destination stream for the indented camel-case document.</param>
    /// <param name="document">Ordered visual block definitions to encode.</param>
    /// <param name="expectedNativeByteCount">Required native table length used to validate the serialized document's block count.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document or expected table length fails metatile validation.</exception>
    public static void Write(Stream json, RoomMetatileDocument document, int expectedNativeByteCount)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), expectedNativeByteCount);
        json.Write(bytes);
    }

    /// <summary>Strict camel-case JSON settings for editable metatile documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
