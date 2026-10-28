using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmGreyDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmGreyDoorVisualsAccess
{
    /// <summary>Creates an uninitialized grey-door art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmGreyDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmGreyDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmGreyDoorVisuals();
    }
}
