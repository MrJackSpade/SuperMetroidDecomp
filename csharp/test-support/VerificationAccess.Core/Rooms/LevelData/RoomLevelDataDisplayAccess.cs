namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to what a room's grid shows in the foreground (#1346, #1431, #1469).</summary>
internal static class RoomLevelDataDisplayAccess
{
    /// <summary>Reads the block the foreground shows.</summary>
    extension(RoomLevelData level)
    {
        /// <summary>
        /// What the foreground shows at <paramref name="blockIndex"/>: the block and its mirroring, which name the picture
        /// the room draws there.
        /// </summary>
        internal RoomBlockAppearance DisplayedAppearance(int blockIndex) => level.ForegroundPlane.Blocks[blockIndex];
    }
}
