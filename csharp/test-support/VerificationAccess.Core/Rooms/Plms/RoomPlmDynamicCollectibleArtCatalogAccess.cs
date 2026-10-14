namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmDynamicCollectibleArtCatalog"/> members production does not use.</summary>
internal static class RoomPlmDynamicCollectibleArtCatalogAccess
{
    /// <summary>Creates an uninitialized dynamic collectible-art fixture for PLM binding checks.</summary>
    extension(RoomPlmDynamicCollectibleArtCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmDynamicCollectibleArtCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmDynamicCollectibleArt();
    }
}
