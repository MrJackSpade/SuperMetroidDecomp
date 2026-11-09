using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectLinkedRestoreVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectLinkedRestoreVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectLinkedRestoreVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectLinkedRestoreVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectLinkedRestoreVisuals();
    }
}
