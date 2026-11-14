using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to what a <see cref="RoomLevelPlane"/> shows, read from the cells a frame draws (#1538).</summary>
internal static class RoomLevelPlaneAccess
{
    /// <summary>Each block's shown appearance.</summary>
    extension(RoomLevelPlane self)
    {
        /// <summary>
        /// What each block shows, row by row, as the cells a frame draws name it: its block and mirroring. Each read builds
        /// the whole plane's list, so a check reading one block at a time reads <see cref="BlockAt"/> instead.
        /// </summary>
        /// <exception cref="InvalidOperationException">A block shows a collectible's picture (#1764).</exception>
        internal RoomBlockAppearance[] Blocks => [.. self.Cells.Cells.Select(cell => new RoomBlockAppearance(cell.Picture.Block, cell.Flips))];

        /// <summary>What block <paramref name="blockIndex"/> shows, as the plane's blocks name it.</summary>
        /// <exception cref="InvalidOperationException">The block shows a collectible's picture (#1764).</exception>
        internal RoomBlockAppearance BlockAt(int blockIndex)
        {
            RoomBlockCell cell = self.Cells.Cells[blockIndex];
            return new RoomBlockAppearance(cell.Picture.Block, cell.Flips);
        }
    }
}
