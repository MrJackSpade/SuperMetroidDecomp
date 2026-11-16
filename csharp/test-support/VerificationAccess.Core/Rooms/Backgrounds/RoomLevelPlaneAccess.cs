using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to what a <see cref="RoomLevelPlane"/> shows, read from the cells a frame draws (#1538).</summary>
internal static class RoomLevelPlaneAccess
{
    /// <summary>Each block's shown appearance.</summary>
    extension(RoomLevelPlane self)
    {
        /// <summary>
        /// What each block shows, row by row, as the cells a frame draws name it: its block or nothing, and its mirroring.
        /// Each read builds the whole plane's list, so a check reading one block at a time reads <see cref="BlockAt"/>
        /// instead.
        /// </summary>
        /// <exception cref="InvalidOperationException">A block shows a collectible's picture (#1764).</exception>
        internal RoomBlockAppearance[] Blocks => [.. self.Cells.Cells.Select(cell => cell.Appearance)];

        /// <summary>What block <paramref name="blockIndex"/> shows, as the plane's blocks name it.</summary>
        /// <exception cref="InvalidOperationException">The block shows a collectible's picture (#1764).</exception>
        internal RoomBlockAppearance BlockAt(int blockIndex) => self.Cells.Cells[blockIndex].Appearance;
    }

    /// <summary>What a cell shows, as a block's appearance.</summary>
    extension(RoomBlockCell cell)
    {
        /// <summary>The cell's block or nothing (#2100), mirrored as the cell is.</summary>
        /// <exception cref="InvalidOperationException">The cell shows a collectible's picture (#1764).</exception>
        internal RoomBlockAppearance Appearance =>
            (cell.Picture.ShowsNothing ? RoomBlockAppearance.Nothing : RoomBlockAppearance.Of(cell.Picture.Block)).WithMirroring(cell.Flips);
    }
}
