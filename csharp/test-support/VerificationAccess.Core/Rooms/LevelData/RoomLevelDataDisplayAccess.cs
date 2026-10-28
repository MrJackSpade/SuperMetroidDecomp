namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to what a room's level data shows on BG1 (#1346, #1431).</summary>
internal static class RoomLevelDataDisplayAccess
{
    /// <summary>Reads the block BG1 shows.</summary>
    extension(RoomLevelData level)
    {
        /// <summary>
        /// The visual word of the block BG1 shows at <paramref name="blockIndex"/>: the block's index with its mirroring, which
        /// names the picture the room draws there.
        /// </summary>
        internal ushort DisplayedVisualWord(int blockIndex) =>
            new RoomLevelWord(level.ForegroundPlane.Blocks[blockIndex]).VisualWord;
    }
}
