namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess
{
    /// <summary>Reads a room's grid and restores it from a fixture snapshot.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>Each foreground block's terrain, row by row.</summary>
        internal ReadOnlySpan<RoomTerrainBlock> TerrainGrid => self.Terrain;

        /// <summary>Each foreground block's behaviour, row by row.</summary>
        internal ReadOnlySpan<RoomBlockBehavior> BehaviorGrid => self.Behaviors;

        /// <summary>Whether this grid's terrain and behaviours, which gameplay reads, equal <paramref name="other"/>'s.</summary>
        internal bool SameTerrainAs(RoomLevelData other) =>
            self.Size == Ensure.NotNull(other).Size &&
            self.Terrain.AsSpan().SequenceEqual(other.Terrain) &&
            self.Behaviors.AsSpan().SequenceEqual(other.Behaviors);

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
            // A terrain write whose terrain and shown picture the block already holds, byte for byte, changes nothing but
            // dropping the published foreground, which every write drops, so only the first block and the blocks whose
            // bytes differ are written (#1757): a check restoring a room per case wrote every block of it per case. A
            // behaviour write inside the grid stores the behaviour at its own place, so the grid's behaviours are copied.
            int index = 0;
            while (index < source.Terrain.Length)
            {
                self.SetObjectTerrain(index, source.Terrain[index], source.ShownForeground[index]);
                index = Math.Min(NextDifference(self.Terrain, source.Terrain, index + 1),
                    NextDifference(self.ShownForeground, source.ShownForeground, index + 1));
            }
            source.Behaviors.AsSpan().CopyTo(self.Behaviors);
            for (int beyond = 0; beyond < source.BeyondRoom.Count; beyond++)
            {
                self.BeyondRoom.SetTerrain(beyond, source.BeyondRoom.TerrainAt(beyond));
                self.BeyondRoom.SetBehavior(beyond, source.BeyondRoom.BehaviorAt(beyond));
            }
            self.PendingDoorTransition = source.PendingDoorTransition;
            self.ElevatorDoorContactPending = source.ElevatorDoorContactPending;
        }
    }

    /// <summary>
    /// The first place from <paramref name="from"/> on whose element's bytes differ between <paramref name="now"/> and
    /// <paramref name="then"/>, or their length when none does.
    /// </summary>
    private static int NextDifference<T>(T[] now, T[] then, int from) where T : unmanaged
    {
        ReadOnlySpan<byte> left = System.Runtime.InteropServices.MemoryMarshal.AsBytes(now.AsSpan(from));
        ReadOnlySpan<byte> right = System.Runtime.InteropServices.MemoryMarshal.AsBytes(then.AsSpan(from));
        int common = left.CommonPrefixLength(right);
        return common == left.Length ? now.Length : from + common / System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
    }
}
