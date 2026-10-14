namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmColoredDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmColoredDoorVisualCatalogAccess
{
    /// <summary>Creates an uninitialized colored-door art fixture for PLM binding checks.</summary>
    extension(RoomPlmColoredDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmColoredDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmColoredDoorVisuals();
    }
}
