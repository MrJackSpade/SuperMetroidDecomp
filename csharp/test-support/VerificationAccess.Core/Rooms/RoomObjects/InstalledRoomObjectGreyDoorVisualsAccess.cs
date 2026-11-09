using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectGreyDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectGreyDoorVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectGreyDoorVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectGreyDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectGreyDoorVisuals();
    }
}
