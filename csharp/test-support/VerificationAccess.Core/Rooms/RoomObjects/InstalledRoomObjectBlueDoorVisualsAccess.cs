using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectBlueDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBlueDoorVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectBlueDoorVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectBlueDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectBlueDoorVisuals();
    }
}
