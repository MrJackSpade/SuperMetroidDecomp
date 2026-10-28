using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmEyeDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmEyeDoorVisualsAccess
{
    /// <summary>Creates an uninitialized eye-door art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmEyeDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmEyeDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmEyeDoorVisuals();
    }
}
