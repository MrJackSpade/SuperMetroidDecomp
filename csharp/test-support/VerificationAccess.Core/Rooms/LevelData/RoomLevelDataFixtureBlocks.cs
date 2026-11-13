namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Fixture block addressing for verification (#1628): the fixtures build a room's planes row by row and name their blocks
/// by place in those planes, a block's row times the room's width plus its column. These members turn such a place into
/// the block it names and read or write the room through its block-addressed members, so production addresses blocks by
/// position only.
/// </summary>
internal static class RoomLevelDataFixtureBlocks
{
    /// <summary>Reads and writes a room's blocks by their place in its planes.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>The place in the planes of <paramref name="block"/>, which must be a block of the grid.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The block lies outside the room's width or height.</exception>
        internal int GetBlockIndex(Position16<RoomBlocks> block) =>
            self.Size.RowMajorIndex(self.GridBlock(block));

        /// <summary>The block at <paramref name="place"/> in the planes, in the room's columns.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is negative.</exception>
        internal Position16<RoomBlocks> BlockAt(int place) =>
            place >= 0
                ? self.Size.CellAtRowMajorIndex(place)
                : throw new ArgumentOutOfRangeException(nameof(place), place, "A fixture block's place is negative.");

        /// <summary>Reads the block of the grid at <paramref name="place"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is outside the grid.</exception>
        internal RoomCollisionBlock GetCollisionBlockByIndex(int place) =>
            self.GetCollisionBlock(self.GridBlockAt(place));

        /// <summary>Reads a room object's block at <paramref name="place"/>, inside the grid or in the terrain beyond the room.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is negative or past the terrain beyond the room.</exception>
        internal RoomCollisionBlock GetObjectCollisionBlockByIndex(int place) =>
            self.GetObjectCollisionBlock(self.BlockAt(place));

        /// <summary>Replaces the terrain of the block of the grid at <paramref name="place"/> and shows its picture.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is outside the grid.</exception>
        internal void SetTerrain(int place, RoomTerrainBlock terrain) =>
            self.SetTerrain(self.GridBlockAt(place), terrain);

        /// <summary>Replaces the behaviour of the block of the grid at <paramref name="place"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is outside the grid.</exception>
        internal void SetBehavior(int place, RoomBlockBehavior behavior) =>
            self.SetBehavior(self.GridBlockAt(place), behavior);

        /// <summary>A room object's terrain write at <paramref name="place"/>, showing the terrain's own picture.</summary>
        internal void SetObjectTerrain(int place, RoomTerrainBlock terrain) =>
            self.SetObjectTerrain(self.BlockAt(place), terrain);

        /// <summary>A room object's terrain write at <paramref name="place"/> that shows <paramref name="shown"/>.</summary>
        internal void SetObjectTerrain(int place, RoomTerrainBlock terrain, RoomBlockAppearance shown) =>
            self.SetObjectTerrain(self.BlockAt(place), terrain, shown);

        /// <summary>A room object's behaviour write at <paramref name="place"/>.</summary>
        internal void SetObjectBehavior(int place, RoomBlockBehavior behavior) =>
            self.SetObjectBehavior(self.BlockAt(place), behavior);

        /// <summary>The block of the grid at <paramref name="place"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is outside the grid.</exception>
        private Position16<RoomBlocks> GridBlockAt(int place) =>
            (uint)place < (uint)self.Size.Area
                ? self.Size.CellAtRowMajorIndex(place)
                : throw new ArgumentOutOfRangeException(nameof(place), place, $"The place lies outside the room's {self.Size.Area}-block grid.");
    }
}
