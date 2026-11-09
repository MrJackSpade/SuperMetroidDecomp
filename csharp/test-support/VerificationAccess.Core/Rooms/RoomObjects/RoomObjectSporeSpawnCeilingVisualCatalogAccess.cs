using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSporeSpawnCeilingVisuals"/> members production does not use.</summary>
internal static class RoomObjectSporeSpawnCeilingVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectSporeSpawnCeilingVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectSporeSpawnCeilingVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectSporeSpawnCeilingVisuals();
    }
}
