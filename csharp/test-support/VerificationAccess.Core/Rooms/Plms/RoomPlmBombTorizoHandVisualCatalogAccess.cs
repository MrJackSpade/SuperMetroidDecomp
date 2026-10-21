using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmBombTorizoHandVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBombTorizoHandVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Bomb Torizo hand-art fixture for PLM binding checks.</summary>
    extension(RoomPlmBombTorizoHandVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmBombTorizoHandVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmBombTorizoHandVisuals();
    }
}
