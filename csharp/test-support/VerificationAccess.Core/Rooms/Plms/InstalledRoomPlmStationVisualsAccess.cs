using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmStationVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmStationVisualsAccess
{
    /// <summary>Calculates station stock appearances while retaining only customized frames.</summary>
    extension(InstalledRoomPlmStationVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmStationVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmStationVisuals();
    }
}
