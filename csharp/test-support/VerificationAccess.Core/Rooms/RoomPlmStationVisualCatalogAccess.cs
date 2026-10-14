using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmStationVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmStationVisualCatalogAccess
{
    /// <summary>Calculates station stock appearances while retaining only customized frames.</summary>
    extension(RoomPlmStationVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmStationVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmStationVisuals();
    }
}
