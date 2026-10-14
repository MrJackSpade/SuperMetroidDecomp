using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmLinkedRestoreVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmLinkedRestoreVisualCatalogAccess
{
    /// <summary>Creates an uninitialized linked-block restoration art fixture for PLM binding checks.</summary>
    extension(RoomPlmLinkedRestoreVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmLinkedRestoreVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmLinkedRestoreVisuals();
    }
}
