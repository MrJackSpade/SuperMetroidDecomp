using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmTourianAccessVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmTourianAccessVisualCatalogAccess
{
    /// <summary>Derives Tourian access-row art from physical draw words, retaining only edited frames.</summary>
    extension(RoomPlmTourianAccessVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmTourianAccessVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmTourianAccessVisuals();
    }
}
