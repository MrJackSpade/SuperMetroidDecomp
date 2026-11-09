using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectNoobTubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectNoobTubeVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectNoobTubeVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectNoobTubeVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectNoobTubeVisuals();
    }
}
