using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSamusEaterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSamusEaterVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectSamusEaterVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectSamusEaterVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectSamusEaterVisuals();
    }
}
