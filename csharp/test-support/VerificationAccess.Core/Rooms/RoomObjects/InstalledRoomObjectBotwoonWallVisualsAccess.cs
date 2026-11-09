using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectBotwoonWallVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBotwoonWallVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectBotwoonWallVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectBotwoonWallVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectBotwoonWallVisuals();
    }
}
