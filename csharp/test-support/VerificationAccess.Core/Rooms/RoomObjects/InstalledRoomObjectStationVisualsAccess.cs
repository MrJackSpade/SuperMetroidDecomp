using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectStationVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectStationVisualsAccess
{
    /// <summary>Calculates station stock appearances while retaining only customized frames.</summary>
    extension(InstalledRoomObjectStationVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectStationVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectStationVisuals();
    }
}
