namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmGreyDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmGreyDoorVisualCatalogAccess
{
    /// <summary>Creates an uninitialized grey-door art fixture for PLM binding checks.</summary>
    extension(RoomPlmGreyDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmGreyDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmGreyDoorVisuals();
    }
}
