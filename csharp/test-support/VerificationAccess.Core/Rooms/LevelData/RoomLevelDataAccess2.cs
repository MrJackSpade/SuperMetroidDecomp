using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess2
{
    /// <summary>Exposes <see cref="RoomLevelData"/> state that only verification reads.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// A room object's terrain write at its block that shows <paramref name="shown"/> over the terrain instead of the
        /// terrain's own picture. Past the grid nothing is shown, so only the terrain persists.
        /// </summary>
        internal void SetObjectTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain, RoomBlockAppearance shown) =>
            self.WriteObjectTerrain(self.Size.RowMajorIndex(block), terrain, shown);
    }
}
