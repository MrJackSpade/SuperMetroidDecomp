using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmChozoStatueVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmChozoStatueVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Chozo statue-art fixture for PLM binding checks.</summary>
    extension(RoomPlmChozoStatueVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmChozoStatueVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmChozoStatueVisuals();
    }
}
