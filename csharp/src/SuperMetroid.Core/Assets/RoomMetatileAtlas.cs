using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable 16x16 visual blocks. This catalog contains only four BG tile words per
/// block; room collision types, BTS behavior and placement remain elsewhere.
/// </summary>
public sealed class RoomMetatileAtlas
{
    /// <summary>Compiled native tilemap words for each 16x16 block, stored as four little-endian BG words in top-left, top-right, bottom-left, bottom-right order.</summary>
    private readonly byte[] blockDefinitions;

    /// <summary>Wraps the already validated native block table produced by <see cref="Load"/>.</summary>
    /// <param name="blockDefinitions">Compiled eight-byte definitions whose order matches the source document.</param>
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
        ArgumentNullException.ThrowIfNull(json);
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
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), expectedNativeByteCount);
        json.Write(bytes);
    }

    /// <summary>Shared JSON policy: camel-case names, case-insensitive input, rejection of unknown fields, and indented output.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Editable JSON schema for one common-room-element or tileset-specific table of 16x16 visual blocks; collision and placement are not included.</summary>
public sealed record RoomMetatileDocument
{
    /// <summary>Schema revision, required to equal <see cref="RoomMetatileFormat.Version"/> when compiling the table.</summary>
    public required int Version { get; init; }
    /// <summary>Non-null, ordered block definitions whose count must match the selected native table length divided by eight.</summary>
    public required RoomMetatileDefinition[] Blocks { get; init; }
}

/// <summary>One 16x16 visual block composed of four independently attributed 8x8 background tiles.</summary>
public sealed record RoomMetatileDefinition
{
    /// <summary>The 8x8 tile at local pixel (0, 0), encoded as the first native tilemap word.</summary>
    public required RoomMetatileCell TopLeft { get; init; }
    /// <summary>The 8x8 tile at local pixel (8, 0), encoded as the second native tilemap word.</summary>
    public required RoomMetatileCell TopRight { get; init; }
    /// <summary>The 8x8 tile at local pixel (0, 8), encoded as the third native tilemap word.</summary>
    public required RoomMetatileCell BottomLeft { get; init; }
    /// <summary>The 8x8 tile at local pixel (8, 8), encoded as the fourth native tilemap word.</summary>
    public required RoomMetatileCell BottomRight { get; init; }
}

/// <summary>Editable components of one SNES BG tilemap word: a ten-bit character index, three-bit palette index, priority, and per-tile flips.</summary>
public sealed record RoomMetatileCell
{
    /// <summary>Zero-based column 0..31 in the logical 32-column character catalog; character index equals TileRow times 32 plus TileColumn.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based row 0..31 in the logical character catalog, selecting one of 1024 possible 8x8 BG characters with <see cref="TileColumn"/>.</summary>
    public required int TileRow { get; init; }
    /// <summary>Background palette index 0..7 encoded in tilemap bits 10..12; colors belong to the separate room palette catalog.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether the BG tilemap priority bit 13 is set; final layer ordering remains the renderer's responsibility.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether this 8x8 character is horizontally mirrored, encoded in bit 14 independently of room-placement block flips.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether this 8x8 character is vertically mirrored, encoded in bit 15 independently of room-placement block flips.</summary>
    public required bool FlipY { get; init; }
}

/// <summary>Schema revision, native block geometry, logical character coordinates, and editable file identities for room metatile tables.</summary>
public static class RoomMetatileFormat
{
    /// <summary>Supported JSON schema revision for editable room block composition.</summary>
    public const int Version = 1;
    /// <summary>Eight bytes per native 16x16 block: four little-endian 16-bit BG tilemap words.</summary>
    public const int BytesPerBlock = RoomAssetRomData.GraphicsLayout.BytesPerBlockDefinition;
    /// <summary>Maximum accepted definitions in one atlas, limiting its compiled native table to $2000 bytes.</summary>
    public const int MaximumBlockCount = 1024;
    /// <summary>Logical character-catalog columns used to split or compose the tilemap word's ten-bit character index.</summary>
    public const int TileColumns = 32;
    /// <summary>Logical character-catalog rows; together with 32 columns these address all 1024 BG character indices.</summary>
    public const int TileRows = 32;
    /// <summary>Eight background palette selections available in a SNES BG tilemap word.</summary>
    public const int PaletteCount = 8;
    /// <summary>Editable common-room-element block filename, corresponding to native TileTable_CRE at $B9:A09D rather than a tileset-specific table.</summary>
    public const string CreFileName = "room-blocks-cre.json";

    /// <summary>Gets the editable block-table filename keyed by a tileset's native compressed source identity.</summary>
    /// <param name="sourceAddress">Source identity rendered as uppercase hexadecimal with at least six digits; no cartridge read or address validation occurs here.</param>
    public static string SourceFileName(int sourceAddress) => $"room-blocks-{sourceAddress:X6}.json";

    /// <summary>Validates that a native table length contains 1..1024 complete eight-byte definitions and returns its block count.</summary>
    /// <param name="byteCount">Decompressed native table length in bytes.</param>
    /// <returns>The number of complete four-quadrant block definitions.</returns>
    /// <exception cref="InvalidDataException">The length is nonpositive, exceeds $2000 bytes, or is not divisible by eight.</exception>
    public static int ValidateBlockCount(int byteCount)
    {
        if (byteCount <= 0 || byteCount > MaximumBlockCount * BytesPerBlock ||
            byteCount % BytesPerBlock != 0)
            throw new InvalidDataException(
                $"Room metatiles require 1..{MaximumBlockCount} complete eight-byte blocks; got {byteCount} bytes.");
        return byteCount / BytesPerBlock;
    }
}
