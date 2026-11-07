using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectCrocomireVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectCrocomireVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Crocomire arena-art fixture for PLM binding checks.</summary>
    extension(RoomObjectCrocomireVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectCrocomireVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectCrocomireVisuals();
    }
}
