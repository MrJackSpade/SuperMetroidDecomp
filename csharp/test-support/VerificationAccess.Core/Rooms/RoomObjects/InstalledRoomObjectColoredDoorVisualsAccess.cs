using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectColoredDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectColoredDoorVisualsAccess
{
    /// <summary>Creates an uninitialized colored-door art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectColoredDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectColoredDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectColoredDoorVisuals();
    }
}
