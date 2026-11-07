using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to what a <see cref="RoomLevelPlane"/> shows, read from the cells a frame draws (#1538).</summary>
internal static class RoomLevelPlaneAccess
{
    /// <summary>Each block's shown appearance.</summary>
    extension(RoomLevelPlane self)
    {
        /// <summary>
        /// What each block shows, row by row, as the cells a frame draws name it: its block and mirroring, or where a
        /// collectible's picture lies, that picture's number unmirrored.
        /// </summary>
        internal RoomBlockAppearance[] Blocks => [.. self.Cells.Cells.Select(cell => new RoomBlockAppearance(cell.Definition, cell.Flips))];
    }
}
