using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmShotBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmShotBlockVisualsAccess
{
    /// <summary>Uses native shot-block draws as the stock visual selection without an installed override.</summary>
    extension(InstalledRoomPlmShotBlockVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmShotBlockVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmShotBlockVisuals();
    }
}
