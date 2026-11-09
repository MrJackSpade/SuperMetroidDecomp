using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectChozoStatueVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectChozoStatueVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(RoomObjectChozoStatueVisualCatalog)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static RoomObjectChozoStatueVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectChozoStatueVisuals();
    }
}
