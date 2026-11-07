using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectTourianAccessVisuals"/> members production does not use.</summary>
internal static class RoomObjectTourianAccessVisualCatalogAccess
{
    /// <summary>Derives Tourian access-row art from physical draw words, retaining only edited frames.</summary>
    extension(RoomObjectTourianAccessVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectTourianAccessVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectTourianAccessVisuals();
    }
}
