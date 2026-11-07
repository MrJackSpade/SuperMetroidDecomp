using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectEyeDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectEyeDoorVisualsAccess
{
    /// <summary>Creates an uninitialized eye-door art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectEyeDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectEyeDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectEyeDoorVisuals();
    }
}
