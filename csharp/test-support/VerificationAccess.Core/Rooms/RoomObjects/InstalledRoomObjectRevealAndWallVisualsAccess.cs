using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectRevealAndWallVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectRevealAndWallVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectRevealAndWallVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectRevealAndWallVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectRevealAndWallVisuals();
    }
}
