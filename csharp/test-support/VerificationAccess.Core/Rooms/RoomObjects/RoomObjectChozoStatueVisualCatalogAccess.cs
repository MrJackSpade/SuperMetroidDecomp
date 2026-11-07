using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectChozoStatueVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectChozoStatueVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Chozo statue-art fixture for PLM binding checks.</summary>
    extension(RoomObjectChozoStatueVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectChozoStatueVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectChozoStatueVisuals();
    }
}
