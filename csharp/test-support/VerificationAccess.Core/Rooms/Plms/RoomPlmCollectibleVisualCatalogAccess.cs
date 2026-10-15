using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmCollectibleVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmCollectibleVisualCatalogAccess
{
    /// <summary>Creates an uninitialized collectible-art fixture for PLM binding checks.</summary>
    extension(RoomPlmCollectibleVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmCollectibleVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmCollectibleVisuals();
    }
}
