using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmDownwardGateVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmDownwardGateVisualsAccess
{
    /// <summary>Creates an uninitialized downward-gate art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmDownwardGateVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmDownwardGateVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmDownwardGateVisuals();
    }
}
