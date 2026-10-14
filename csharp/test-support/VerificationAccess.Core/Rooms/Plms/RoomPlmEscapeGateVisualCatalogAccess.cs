namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmEscapeGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmEscapeGateVisualCatalogAccess
{
    /// <summary>Creates an uninitialized escape-gate art fixture for PLM binding checks.</summary>
    extension(RoomPlmEscapeGateVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmEscapeGateVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmEscapeGateVisuals();
    }
}
