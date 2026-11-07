using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectMotherBrainFakeDeathVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectMotherBrainFakeDeathVisualCatalogAccess
{
    /// <summary>Uses physical fake-death cells as stock art without a second stored draw cache.</summary>
    extension(RoomObjectMotherBrainFakeDeathVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectMotherBrainFakeDeathVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectMotherBrainFakeDeathVisuals();
    }
}
