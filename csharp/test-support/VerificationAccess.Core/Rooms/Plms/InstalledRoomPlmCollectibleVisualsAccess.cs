using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmCollectibleVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmCollectibleVisualsAccess
{
    /// <summary>Creates an uninitialized collectible-art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmCollectibleVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmCollectibleVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmCollectibleVisuals();
    }
}
