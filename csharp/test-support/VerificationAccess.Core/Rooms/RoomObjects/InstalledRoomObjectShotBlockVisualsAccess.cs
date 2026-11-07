using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectShotBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectShotBlockVisualsAccess
{
    /// <summary>Uses native shot-block draws as the stock visual selection without an installed override.</summary>
    extension(InstalledRoomObjectShotBlockVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectShotBlockVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectShotBlockVisuals();
    }
}
