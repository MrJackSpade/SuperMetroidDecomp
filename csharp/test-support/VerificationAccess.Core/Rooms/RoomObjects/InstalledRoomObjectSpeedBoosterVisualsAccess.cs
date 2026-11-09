using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSpeedBoosterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSpeedBoosterVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectSpeedBoosterVisuals)
    {
        /// <summary>The bomb reveal's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectSpeedBoosterVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectSpeedBoosterVisuals();
    }
}
