using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectShotBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectShotBlockVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectShotBlockVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectShotBlockVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectShotBlockVisuals();
    }
}
