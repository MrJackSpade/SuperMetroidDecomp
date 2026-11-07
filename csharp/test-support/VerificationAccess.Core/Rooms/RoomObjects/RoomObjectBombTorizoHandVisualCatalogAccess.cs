using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectBombTorizoHandVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectBombTorizoHandVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Bomb Torizo hand-art fixture for PLM binding checks.</summary>
    extension(RoomObjectBombTorizoHandVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectBombTorizoHandVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectBombTorizoHandVisuals();
    }
}
