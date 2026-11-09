using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectSporeSpawnCeilingVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectSporeSpawnCeilingVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(RoomObjectSporeSpawnCeilingVisualCatalog)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static RoomObjectSporeSpawnCeilingVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectSporeSpawnCeilingVisuals();
    }
}
