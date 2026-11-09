using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectStationVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectStationVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectStationVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectStationVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectStationVisuals();
    }
}
