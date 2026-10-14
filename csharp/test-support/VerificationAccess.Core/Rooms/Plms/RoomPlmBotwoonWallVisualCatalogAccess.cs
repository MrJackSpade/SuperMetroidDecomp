using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmBotwoonWallVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBotwoonWallVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Botwoon wall-art fixture for PLM binding checks.</summary>
    extension(RoomPlmBotwoonWallVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmBotwoonWallVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmBotwoonWallVisuals();
    }
}
