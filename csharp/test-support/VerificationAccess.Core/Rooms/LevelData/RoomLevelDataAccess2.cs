namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelData"/>'s private terrain write (#2482).</summary>
internal static class RoomLevelDataAccess2
{
    /// <summary>Writes a room's terrain with a picture of the fixture's choosing.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// A room object's terrain write at its block that shows <paramref name="shown"/> over the terrain instead of the
        /// terrain's own picture, through the level's own private write. Past the grid nothing is shown, so only the
        /// terrain persists.
        /// </summary>
        internal void SetObjectTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain, RoomBlockAppearance shown) =>
            RoomLevelDataAccess.WriteObjectTerrain(self, self.Size.RowMajorIndex(block), terrain, shown);
    }
}
