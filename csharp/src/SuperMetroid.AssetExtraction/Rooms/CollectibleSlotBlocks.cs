using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The blocks a collectible shows through its character slot (#1431): instruction <c>$84:8764</c> writes, for the slot it
/// loads, the tiles of two common-room-element blocks, one per animation frame, each reading four of the slot's characters
/// in the colour variants the collectible's palette selectors give them. The room names those blocks' pictures by the
/// collectible bound over the slot, so extraction composes them here from what the cartridge writes.
/// </summary>
internal static class CollectibleSlotBlocks
{
    /// <summary>The first block the four slots write: byte offset $0470 of the common room elements' block table.</summary>
    internal const int FirstBlock = 0x0470 / RoomMetatileFormat.BytesPerBlock;

    /// <summary>The blocks one slot writes: one per animation frame.</summary>
    internal const int BlocksPerSlot = RoomTileCharacters.CollectibleSlotCharacters / RoomMetatileFormat.TilesPerBlock;

    /// <summary>The first block slot <paramref name="graphicsSlot"/> writes.</summary>
    internal static int FirstBlockOf(int graphicsSlot) => FirstBlock + graphicsSlot * BlocksPerSlot;

    /// <summary>The slot whose blocks include <paramref name="block"/>, or null for a block no slot writes.</summary>
    /// <param name="block">A common room element block's index.</param>
    /// <param name="graphicsSlots">The slots collectibles rotate through.</param>
    internal static int? SlotOf(int block, int graphicsSlots) =>
        block >= FirstBlock && block < FirstBlock + graphicsSlots * BlocksPerSlot ? (block - FirstBlock) / BlocksPerSlot : null;

    /// <summary>The characters slot block <paramref name="block"/> reads once a collectible writes it: its frame's four.</summary>
    internal static IEnumerable<int> CharactersOf(int block) =>
        Enumerable.Range(RoomTileCharacters.CollectiblesFirst + (block - FirstBlock) * RoomMetatileFormat.TilesPerBlock, RoomMetatileFormat.TilesPerBlock);

    /// <summary>
    /// The tiles a collectible writes for slot <paramref name="graphicsSlot"/>'s two blocks, four per block: each of the
    /// slot's characters in the colour variant <paramref name="paletteOffsets"/> gives it.
    /// </summary>
    internal static BackgroundTile[] Tiles(int graphicsSlot, ReadOnlySpan<byte> paletteOffsets)
    {
        int first = RoomTileCharacters.CollectiblesFirst + graphicsSlot * RoomTileCharacters.CollectibleSlotCharacters;
        var tiles = new BackgroundTile[RoomTileCharacters.CollectibleSlotCharacters];
        for (int child = 0; child < tiles.Length; child++)
            tiles[child] = new BackgroundTile(first + child, paletteOffsets[child], highPriority: false);
        return tiles;
    }
}
