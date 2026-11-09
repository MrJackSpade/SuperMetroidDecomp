using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectCollectibleVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectCollectibleVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectCollectibleVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectCollectibleVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectCollectibleVisuals();
    }
}
