namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to <see cref="RoomBlockCells"/> members production does not use.</summary>
internal static class RoomBlockCellsAccess
{
    /// <summary>Exposes <see cref="RoomBlockCells"/> state that only verification reads.</summary>
    extension(RoomBlockCells self)
    {
        /// <summary>The block at a level pixel and the pixel within it; false outside the room.</summary>
        /// <param name="level">The level pixel; coordinates past the room's right or bottom edge are outside.</param>
        /// <param name="cell">The block there.</param>
        /// <param name="within">The pixel within the block, from its top-left corner, before its mirroring.</param>
        public bool TryCellAt(Position16<BgPlanePixels> level, out RoomBlockCell cell, out Position16<BgPlanePixels> within)
        {
            if (level.X >= self.Size.X * RoomLevelPlane.BlockPixels || level.Y >= self.Size.Y * RoomLevelPlane.BlockPixels)
            {
                cell = default;
                within = default;
                return false;
            }
            cell = self.Cells[level.Y / RoomLevelPlane.BlockPixels * self.Size.X + level.X / RoomLevelPlane.BlockPixels];
            within = new Position16<BgPlanePixels>((ushort)(level.X % RoomLevelPlane.BlockPixels), (ushort)(level.Y % RoomLevelPlane.BlockPixels));
            return true;
        }
    }
}
