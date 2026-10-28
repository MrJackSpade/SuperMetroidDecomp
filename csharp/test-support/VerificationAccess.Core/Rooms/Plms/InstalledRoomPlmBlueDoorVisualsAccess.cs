using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmBlueDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmBlueDoorVisualsAccess
{
    /// <summary>Constructs a blue-door visual catalog fixture without requiring installed art.</summary>
    extension(InstalledRoomPlmBlueDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmBlueDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmBlueDoorVisuals();
    }
}
