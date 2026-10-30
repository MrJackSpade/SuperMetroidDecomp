using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="RoomSkyTilemaps"/> members production does not use.</summary>
internal static class RoomSkyTilemapsAccess
{
    /// <summary>Exposes <see cref="RoomSkyTilemaps"/> state that only verification reads.</summary>
    extension(RoomSkyTilemaps self)
    {
        /// <summary>The tiles of all seven selected sky pages in their scrolling order.</summary>
        public ReadOnlyMemory<BackgroundTile> Pages => self.pages;
    }
}
