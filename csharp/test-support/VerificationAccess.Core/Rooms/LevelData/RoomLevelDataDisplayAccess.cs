using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to what a room's level data shows on BG1 (#1346).</summary>
internal static class RoomLevelDataDisplayAccess
{
    /// <summary>Reads the tiles a block shows.</summary>
    extension(RoomLevelData level)
    {
        /// <summary>The four BG1 tiles the block at <paramref name="blockIndex"/> shows, mirrored by its flips.</summary>
        internal ExpandedBlockTiles DisplayedBlockTiles(int blockIndex) =>
            level.ForegroundPlane.BlockTiles(level.Size.CellAtRowMajorIndex(blockIndex));
    }
}
