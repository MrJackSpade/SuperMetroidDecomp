using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectCollectibleVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectCollectibleVisualsAccess
{
    /// <summary>Creates an uninitialized collectible-art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectCollectibleVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectCollectibleVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectCollectibleVisuals();
    }
}
