using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectMotherBrainGlassVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectMotherBrainGlassVisualCatalogAccess
{
    /// <summary>Leaves Mother Brain glass stock cells calculated while allowing custom frame bindings.</summary>
    extension(RoomObjectMotherBrainGlassVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectMotherBrainGlassVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectMotherBrainGlassVisuals();
    }
}
