
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="RoomBackgroundTilemaps"/> members production does not use.</summary>
internal static class RoomBackgroundTilemapsAccess
{
    /// <summary>Exposes <see cref="RoomBackgroundTilemaps"/> state that only verification reads.</summary>
    extension(RoomBackgroundTilemaps self)
    {
        /// <summary>Every selected tilemap, by the background it pictures.</summary>
        public IReadOnlyDictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas> Tilemaps => self.byTilemap;
    }
}
