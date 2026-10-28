using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmTourianAccessVisuals"/> members production does not use.</summary>
internal static class RoomPlmTourianAccessVisualCatalogAccess
{
    /// <summary>Derives Tourian access-row art from physical draw words, retaining only edited frames.</summary>
    extension(RoomPlmTourianAccessVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmTourianAccessVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmTourianAccessVisuals();
    }
}
