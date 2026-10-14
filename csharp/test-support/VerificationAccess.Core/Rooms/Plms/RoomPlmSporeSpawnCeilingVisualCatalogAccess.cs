namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmSporeSpawnCeilingVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSporeSpawnCeilingVisualCatalogAccess
{
    /// <summary>Calculates stock ceiling-crumble squares from their physical draw words.</summary>
    extension(RoomPlmSporeSpawnCeilingVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmSporeSpawnCeilingVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmSporeSpawnCeilingVisuals();
    }
}
