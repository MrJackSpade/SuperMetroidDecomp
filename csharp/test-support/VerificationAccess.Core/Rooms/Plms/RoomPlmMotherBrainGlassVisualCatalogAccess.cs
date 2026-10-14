namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmMotherBrainGlassVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMotherBrainGlassVisualCatalogAccess
{
    /// <summary>Leaves Mother Brain glass stock cells calculated while allowing custom frame bindings.</summary>
    extension(RoomPlmMotherBrainGlassVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmMotherBrainGlassVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmMotherBrainGlassVisuals();
    }
}
