using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmMotherBrainFakeDeathVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMotherBrainFakeDeathVisualCatalogAccess
{
    /// <summary>Uses physical fake-death cells as stock art without a second stored draw cache.</summary>
    extension(RoomPlmMotherBrainFakeDeathVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmMotherBrainFakeDeathVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
    }
}
