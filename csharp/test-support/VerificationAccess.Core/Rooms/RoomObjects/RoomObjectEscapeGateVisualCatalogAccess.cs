using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectEscapeGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectEscapeGateVisualCatalogAccess
{
    /// <summary>Creates an uninitialized escape-gate art fixture for PLM binding checks.</summary>
    extension(RoomObjectEscapeGateVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectEscapeGateVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectEscapeGateVisuals();
    }
}
