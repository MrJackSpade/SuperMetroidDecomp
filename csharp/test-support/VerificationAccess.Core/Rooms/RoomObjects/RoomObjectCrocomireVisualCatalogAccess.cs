using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectCrocomireVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectCrocomireVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(RoomObjectCrocomireVisualCatalog)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static RoomObjectCrocomireVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectCrocomireVisuals();
    }
}
