using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="RoomMetatileCatalog"/> members production does not use.</summary>
internal static class RoomMetatileCatalogAccess
{
    /// <summary>Exposes <see cref="RoomMetatileCatalog"/> state that only verification reads.</summary>
    extension(RoomMetatileCatalog self)
    {
        /// <summary>Every installed block table besides the common room elements.</summary>
        public IReadOnlyDictionary<RoomBlockSet, RoomMetatileAtlas> Atlases => self.byRoomBlockSet;
    }
}
