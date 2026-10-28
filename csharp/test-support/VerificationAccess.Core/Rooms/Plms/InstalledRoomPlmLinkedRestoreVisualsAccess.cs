using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmLinkedRestoreVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmLinkedRestoreVisualsAccess
{
    /// <summary>Creates an uninitialized linked-block restoration art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmLinkedRestoreVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmLinkedRestoreVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmLinkedRestoreVisuals();
    }
}
