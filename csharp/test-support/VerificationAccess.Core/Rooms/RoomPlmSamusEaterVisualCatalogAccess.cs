using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmSamusEaterVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSamusEaterVisualCatalogAccess
{
    /// <summary>Derives Samus Eater stock appearance from physical draw words without caching art.</summary>
    extension(RoomPlmSamusEaterVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmSamusEaterVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmSamusEaterVisuals();
    }
}
