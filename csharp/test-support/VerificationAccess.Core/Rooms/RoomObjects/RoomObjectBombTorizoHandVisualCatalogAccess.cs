using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectBombTorizoHandVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectBombTorizoHandVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(RoomObjectBombTorizoHandVisualCatalog)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static RoomObjectBombTorizoHandVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectBombTorizoHandVisuals();
    }
}
