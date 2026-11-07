using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectSporeSpawnCeilingVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectSporeSpawnCeilingVisualCatalogAccess
{
    /// <summary>Calculates stock ceiling-crumble squares from their physical draw words.</summary>
    extension(RoomObjectSporeSpawnCeilingVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectSporeSpawnCeilingVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectSporeSpawnCeilingVisuals();
    }
}
