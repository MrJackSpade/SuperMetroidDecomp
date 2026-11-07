using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectDownwardGateVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectDownwardGateVisualsAccess
{
    /// <summary>Creates an uninitialized downward-gate art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectDownwardGateVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectDownwardGateVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectDownwardGateVisuals();
    }
}
