namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess3
{
    /// <summary>Exposes <see cref="RoomLevelData"/> state that only verification reads.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// A room object's behaviour write at its block, inside the grid or in the terrain beyond the room. A block of the grid
        /// no longer leads where its earlier behaviour did.
        /// </summary>
        internal void SetObjectBehavior(Position16<RoomBlocks> block, RoomBlockBehavior behavior)
        {
            int place = self.Size.RowMajorIndex(block);
            if (place >= self.terrain.Length)
            {
                self.BeyondRoom.SetBehavior(self.BeyondPlace(place), behavior);
                return;
            }

            self.WriteBehavior(place, behavior);
        }
    }
}
