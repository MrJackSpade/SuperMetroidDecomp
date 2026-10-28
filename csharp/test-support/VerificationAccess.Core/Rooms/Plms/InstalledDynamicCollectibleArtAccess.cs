using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledDynamicCollectibleArt"/> members production does not use.</summary>
internal static class InstalledDynamicCollectibleArtAccess
{
    /// <summary>Creates an uninitialized dynamic collectible-art fixture for PLM binding checks.</summary>
    extension(InstalledDynamicCollectibleArt)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledDynamicCollectibleArt Stock() => RepositoryInstallation.Installation.LoadRoomPlmDynamicCollectibleArt();
    }
}
