using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmBlueDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBlueDoorVisualCatalogAccess
{
    /// <summary>Constructs a blue-door visual catalog fixture without requiring installed art.</summary>
    extension(RoomPlmBlueDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmBlueDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmBlueDoorVisuals();
    }
}
