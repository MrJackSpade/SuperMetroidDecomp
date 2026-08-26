using System.Buffers.Binary;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Expands one Super Metroid 16x16 level entry into four SNES 8x8 tilemap words.
/// </summary>
/// <remarks>
/// This is the common inner operation in the column streamer at <c>$80:AA95</c>, the row
/// streamer at <c>$80:AC57</c>, and the desktop room renderer. Centralizing it prevents the
/// live tilemap and exported PNG paths from acquiring subtly different flip semantics.
/// </remarks>
public static class LevelBlockTilemapExpander
{
    /// <summary>
    /// Reads the indexed eight-byte definition from the combined CRE/area block table and
    /// applies the parent level entry's horizontal and vertical flips.
    /// </summary>
    public static ExpandedBlockTiles Expand(ushort levelEntry, ReadOnlySpan<byte> blockDefinitions)
    {
        // Bits 0-9 select a block definition. Bits 10-11 flip the entire 16x16 block.
        // Higher collision/type bits are intentionally irrelevant to visual expansion.
        int blockIndex = levelEntry & 0x03ff;
        int definitionOffset = blockIndex * 8;
        if (definitionOffset + 8 > blockDefinitions.Length)
        {
            throw new InvalidDataException(
                $"Block index ${blockIndex:X3} exceeds the {blockDefinitions.Length / 8} loaded definitions.");
        }

        ushort topLeft = BinaryPrimitives.ReadUInt16LittleEndian(blockDefinitions[definitionOffset..]);
        ushort topRight = BinaryPrimitives.ReadUInt16LittleEndian(blockDefinitions[(definitionOffset + 2)..]);
        ushort bottomLeft = BinaryPrimitives.ReadUInt16LittleEndian(blockDefinitions[(definitionOffset + 4)..]);
        ushort bottomRight = BinaryPrimitives.ReadUInt16LittleEndian(blockDefinitions[(definitionOffset + 6)..]);

        // The XORs do two jobs at once: the four children move to their mirrored quadrants,
        // and each 8x8 child's own PPU flip flag toggles so pixels mirror inside that child.
        // The case order mirrors the native comparisons against $0400 and $0800.
        return (levelEntry & 0x0c00) switch
        {
            0x0000 => new ExpandedBlockTiles(topLeft, topRight, bottomLeft, bottomRight),
            0x0400 => new ExpandedBlockTiles(
                (ushort)(topRight ^ 0x4000),
                (ushort)(topLeft ^ 0x4000),
                (ushort)(bottomRight ^ 0x4000),
                (ushort)(bottomLeft ^ 0x4000)),
            0x0800 => new ExpandedBlockTiles(
                (ushort)(bottomLeft ^ 0x8000),
                (ushort)(bottomRight ^ 0x8000),
                (ushort)(topLeft ^ 0x8000),
                (ushort)(topRight ^ 0x8000)),
            _ => new ExpandedBlockTiles(
                (ushort)(bottomRight ^ 0xc000),
                (ushort)(bottomLeft ^ 0xc000),
                (ushort)(topRight ^ 0xc000),
                (ushort)(topLeft ^ 0xc000)),
        };
    }
}

/// <summary>Four row-major SNES tilemap words forming a visual 16x16 block.</summary>
public readonly record struct ExpandedBlockTiles(
    ushort TopLeft,
    ushort TopRight,
    ushort BottomLeft,
    ushort BottomRight);
