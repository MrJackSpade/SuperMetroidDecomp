using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectLinkedRestoreVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectLinkedRestoreVisualsAccess
{
    /// <summary>Creates an uninitialized linked-block restoration art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectLinkedRestoreVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectLinkedRestoreVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectLinkedRestoreVisuals();
    }
}
