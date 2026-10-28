using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmSamusEaterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmSamusEaterVisualsAccess
{
    /// <summary>Derives Samus Eater stock appearance from physical draw words without caching art.</summary>
    extension(InstalledRoomPlmSamusEaterVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmSamusEaterVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmSamusEaterVisuals();
    }
}
