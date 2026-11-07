using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSamusEaterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSamusEaterVisualsAccess
{
    /// <summary>Derives Samus Eater stock appearance from physical draw words without caching art.</summary>
    extension(InstalledRoomObjectSamusEaterVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectSamusEaterVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectSamusEaterVisuals();
    }
}
