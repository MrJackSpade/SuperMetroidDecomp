using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;

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
    public static ExpandedBlockTiles Expand(RoomLevelWord levelEntry, ReadOnlySpan<byte> blockDefinitions)
    {
        // The typed word keeps the collision nibble explicitly out of visual expansion.
        int blockIndex = levelEntry.VisualBlockIndex;
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
        // The case order mirrors the native horizontal/vertical flag comparisons.
        return levelEntry.VisualFlipFlags switch
        {
            LevelBlockFlipFlags.None =>
                new ExpandedBlockTiles(topLeft, topRight, bottomLeft, bottomRight),
            LevelBlockFlipFlags.Horizontal => new ExpandedBlockTiles(
                new SnesBgTilemapWord(topRight).ToggleFlips(SnesTileFlipFlags.Horizontal),
                new SnesBgTilemapWord(topLeft).ToggleFlips(SnesTileFlipFlags.Horizontal),
                new SnesBgTilemapWord(bottomRight).ToggleFlips(SnesTileFlipFlags.Horizontal),
                new SnesBgTilemapWord(bottomLeft).ToggleFlips(SnesTileFlipFlags.Horizontal)),
            LevelBlockFlipFlags.Vertical => new ExpandedBlockTiles(
                new SnesBgTilemapWord(bottomLeft).ToggleFlips(SnesTileFlipFlags.Vertical),
                new SnesBgTilemapWord(bottomRight).ToggleFlips(SnesTileFlipFlags.Vertical),
                new SnesBgTilemapWord(topLeft).ToggleFlips(SnesTileFlipFlags.Vertical),
                new SnesBgTilemapWord(topRight).ToggleFlips(SnesTileFlipFlags.Vertical)),
            LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical => new ExpandedBlockTiles(
                new SnesBgTilemapWord(bottomRight).ToggleFlips(
                    SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical),
                new SnesBgTilemapWord(bottomLeft).ToggleFlips(
                    SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical),
                new SnesBgTilemapWord(topRight).ToggleFlips(
                    SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical),
                new SnesBgTilemapWord(topLeft).ToggleFlips(
                    SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical)),
            _ => throw new InvalidDataException(
                $"Level word ${levelEntry.Raw:X4} contains unsupported visual flip flags."),
        };
    }
}

/// <summary>Four row-major SNES tilemap words forming a visual 16x16 block.</summary>
public readonly record struct ExpandedBlockTiles(
    ushort TopLeft,
    ushort TopRight,
    ushort BottomLeft,
    ushort BottomRight);
