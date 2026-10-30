namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess
{
    /// <summary>Restores a room's grid from a fixture snapshot.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// Returns this level to <paramref name="source"/>'s complete state in place: every plane, the terrain beyond the
        /// room and the pending door flags. Both levels must share one layout.
        /// </summary>
        internal void RestoreFrom(RoomLevelData source)
        {
            Ensure.NotNull(source);
            if (source.Size.X != self.Size.X || source.Size.Y != self.Size.Y ||
                !ReferenceEquals(source.Doors, self.Doors) ||
                source.BeyondRoom.Count != self.BeyondRoom.Count)
                throw new ArgumentException("A level can only be restored from one with the same layout.", nameof(source));
            for (int index = 0; index < source.Terrain.Length; index++)
            {
                self.SetPlmTerrain(index, source.Terrain[index], source.ShownForeground[index]);
                self.SetPlmBehavior(index, source.Behaviors[index]);
            }
            for (int beyond = 0; beyond < source.BeyondRoom.Count; beyond++)
            {
                self.BeyondRoom.SetTerrain(beyond, source.BeyondRoom.TerrainAt(beyond));
                self.BeyondRoom.SetBehavior(beyond, source.BeyondRoom.BehaviorAt(beyond));
            }
            self.PendingDoorTransition = source.PendingDoorTransition;
            self.ElevatorDoorContactPending = source.ElevatorDoorContactPending;
        }
    }
}
