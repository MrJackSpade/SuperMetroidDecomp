using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectGreyDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectGreyDoorVisualsAccess
{
    /// <summary>Creates an uninitialized grey-door art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectGreyDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectGreyDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectGreyDoorVisuals();
    }
}
