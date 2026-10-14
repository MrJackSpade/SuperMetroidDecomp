namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmCrocomireVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmCrocomireVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Crocomire arena-art fixture for PLM binding checks.</summary>
    extension(RoomPlmCrocomireVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmCrocomireVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmCrocomireVisuals();
    }
}
