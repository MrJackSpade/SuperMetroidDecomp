namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Verification access to <see cref="RoomLevelData"/>'s private grid (#2482): its planes are its own, so a check reads them,
/// and a fixture restores them, through reflection by name bound once, as #2355 reaches private state.
/// </summary>
internal static class RoomLevelDataAccess
{
    /// <summary>Reads a room's grid and restores it from a fixture snapshot.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>Each foreground block's terrain, row by row.</summary>
        internal ReadOnlySpan<RoomTerrainBlock> TerrainGrid => TerrainOf(self);

        /// <summary>Each foreground block's behaviour, row by row.</summary>
        internal ReadOnlySpan<RoomBlockBehavior> BehaviorGrid => BehaviorsOf(self);

        /// <summary>Where each foreground block leads as a door block, row by row, or null where it leads nowhere.</summary>
        internal ReadOnlySpan<Doors.DoorBlockExit?> ExitGrid => ExitsOf(self);

        /// <summary>What each foreground block shows, row by row.</summary>
        internal ReadOnlySpan<RoomBlockAppearance> ShownForegroundGrid => ShownForegroundOf(self);

        /// <summary>The first block of the grid, in reading order, that leads through <paramref name="exit"/> as a door block.</summary>
        /// <exception cref="InvalidOperationException">No door block of the grid leads through it.</exception>
        internal RoomCollisionBlock DoorBlockLeadingThrough(Doors.DoorBlockExit exit)
        {
            int place = Array.IndexOf(ExitsOf(self), exit);
            return place >= 0 && TerrainOf(self)[place].Collision == RoomCollisionType.DoorBlock
                ? self.GetCollisionBlock(self.Size.CellAtRowMajorIndex(place))
                : throw new InvalidOperationException($"No door block of the room leads through {exit}.");
        }

        /// <summary>Whether this grid's terrain and behaviours, which gameplay reads, equal <paramref name="other"/>'s.</summary>
        internal bool SameTerrainAs(RoomLevelData other) =>
            self.Size == Ensure.NotNull(other).Size &&
            TerrainOf(self).AsSpan().SequenceEqual(TerrainOf(other)) &&
            BehaviorsOf(self).AsSpan().SequenceEqual(BehaviorsOf(other));

        /// <summary>
        /// Returns this level to <paramref name="source"/>'s complete state in place: every plane, the terrain beyond the
        /// room. Both levels must share one layout; the room's door contacts are its room objects', not its level's (#2545).
        /// </summary>
        internal void RestoreFrom(RoomLevelData source)
        {
            Ensure.NotNull(source);
            if (source.Size.X != self.Size.X || source.Size.Y != self.Size.Y ||
                source.BeyondRoom.Count != self.BeyondRoom.Count)
                throw new ArgumentException("A level can only be restored from one with the same layout.", nameof(source));
            RoomTerrainBlock[] sourceTerrain = TerrainOf(source);
            RoomBlockAppearance[] sourceShown = ShownForegroundOf(source);
            RoomTerrainBlock[] terrain = TerrainOf(self);
            RoomBlockAppearance[] shown = ShownForegroundOf(self);
            // A terrain write whose terrain and shown picture the block already holds, byte for byte, changes nothing but
            // dropping the published foreground, which every write drops, so only the first block and the blocks whose
            // bytes differ are written (#1757): a check restoring a room per case wrote every block of it per case. A
            // behaviour write inside the grid stores the behaviour at its own place, so the grid's behaviours are copied.
            int index = 0;
            while (index < sourceTerrain.Length)
            {
                WriteObjectTerrain(self, index, sourceTerrain[index], sourceShown[index]);
                index = Math.Min(NextDifference(terrain, sourceTerrain, index + 1),
                    NextDifference(shown, sourceShown, index + 1));
            }
            BehaviorsOf(source).AsSpan().CopyTo(BehaviorsOf(self));
            ExitsOf(source).AsSpan().CopyTo(ExitsOf(self));
            for (int beyond = 0; beyond < source.BeyondRoom.Count; beyond++)
            {
                self.BeyondRoom.SetTerrain(beyond, source.BeyondRoom.TerrainAt(beyond));
                self.BeyondRoom.SetBehavior(beyond, source.BeyondRoom.BehaviorAt(beyond));
            }
        }
    }

    /// <summary>
    /// Writes <paramref name="terrain"/> at <paramref name="place"/> of <paramref name="level"/>'s level space, showing
    /// <paramref name="shown"/>, through the level's own terrain write.
    /// </summary>
    internal static void WriteObjectTerrain(RoomLevelData level, int place, RoomTerrainBlock terrain, RoomBlockAppearance shown) =>
        Ensure.NotNull(level).WriteObjectTerrain(place, terrain, shown);

    /// <summary>The grid's terrain array of <paramref name="level"/>, which a restore writes.</summary>
    private static RoomTerrainBlock[] TerrainOf(RoomLevelData level) => Ensure.NotNull(level).terrain;

    /// <summary>The grid's behaviour array of <paramref name="level"/>, which a restore writes.</summary>
    private static RoomBlockBehavior[] BehaviorsOf(RoomLevelData level) => Ensure.NotNull(level).behaviors;

    /// <summary>The grid's door-exit array of <paramref name="level"/>, which a restore writes.</summary>
    private static Doors.DoorBlockExit?[] ExitsOf(RoomLevelData level) => Ensure.NotNull(level).exits;

    /// <summary>The grid's shown-foreground array of <paramref name="level"/>.</summary>
    private static RoomBlockAppearance[] ShownForegroundOf(RoomLevelData level) => Ensure.NotNull(level).shownForeground;

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
