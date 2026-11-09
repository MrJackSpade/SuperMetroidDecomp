using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectColoredDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectColoredDoorVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectColoredDoorVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectColoredDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectColoredDoorVisuals();
    }
}
