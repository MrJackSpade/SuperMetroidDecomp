namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmDownwardGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmDownwardGateVisualCatalogAccess
{
    /// <summary>Creates an uninitialized downward-gate art fixture for PLM binding checks.</summary>
    extension(RoomPlmDownwardGateVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmDownwardGateVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmDownwardGateVisuals();
    }
}
