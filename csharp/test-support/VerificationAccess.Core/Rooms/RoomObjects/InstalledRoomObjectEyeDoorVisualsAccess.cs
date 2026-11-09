using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectEyeDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectEyeDoorVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectEyeDoorVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectEyeDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectEyeDoorVisuals();
    }
}
