using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;
/// <summary>Verification access to <see cref="InstalledRoomObjectKraidVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectKraidVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectKraidVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectKraidVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectKraidVisuals();
    }
}
