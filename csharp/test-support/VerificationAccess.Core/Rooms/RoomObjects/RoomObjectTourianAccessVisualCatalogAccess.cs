using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectTourianAccessVisuals"/> members production does not use.</summary>
internal static class RoomObjectTourianAccessVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(RoomObjectTourianAccessVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static RoomObjectTourianAccessVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectTourianAccessVisuals();
    }
}
