using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectBotwoonWallVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBotwoonWallVisualsAccess
{
    /// <summary>Creates an uninitialized Botwoon wall-art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectBotwoonWallVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectBotwoonWallVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectBotwoonWallVisuals();
    }
}
