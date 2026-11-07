using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectBlueDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBlueDoorVisualsAccess
{
    /// <summary>Constructs a blue-door visual catalog fixture without requiring installed art.</summary>
    extension(InstalledRoomObjectBlueDoorVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectBlueDoorVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectBlueDoorVisuals();
    }
}
