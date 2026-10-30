namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomCollisionBlock"/> members production does not use.</summary>
internal static class RoomCollisionBlockAccess
{
    /// <summary>Reads the block a collision block's terrain shows.</summary>
    extension(RoomCollisionBlock self)
    {
        /// <summary>The block the terrain shows, by its number in the tileset's block set.</summary>
        internal int VisualBlockIndex => self.Terrain.Appearance.Block;
    }
}
