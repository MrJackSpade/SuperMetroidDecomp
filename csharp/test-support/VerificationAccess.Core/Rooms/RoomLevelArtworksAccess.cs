using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomLevelArtworks"/> members production does not use.</summary>
internal static class RoomLevelArtworksAccess
{
    /// <summary>Constructs partial room-geometry layouts for focused visual fixtures.</summary>
    extension(RoomLevelArtworks)
    {
        /// <summary>Explicitly partial geometry fixtures; never an installed production catalog.</summary>
        internal static RoomLevelArtworks FromLayoutsForVerification(
            IReadOnlyDictionary<RoomLevelId, RoomLevelArtwork> layouts) =>
            new RoomLevelArtworks(layouts, requiredLevels: []);
    }
}
