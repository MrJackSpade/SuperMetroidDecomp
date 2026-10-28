using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmBotwoonWallVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmBotwoonWallVisualsAccess
{
    /// <summary>Creates an uninitialized Botwoon wall-art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmBotwoonWallVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmBotwoonWallVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmBotwoonWallVisuals();
    }
}
