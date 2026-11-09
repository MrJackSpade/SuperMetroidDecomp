using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectDownwardGateVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectDownwardGateVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectDownwardGateVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectDownwardGateVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectDownwardGateVisuals();
    }
}
