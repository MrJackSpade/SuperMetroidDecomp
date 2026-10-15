using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmDraygonCannonVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmDraygonCannonVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Draygon cannon-art fixture for PLM binding checks.</summary>
    extension(RoomPlmDraygonCannonVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmDraygonCannonVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmDraygonCannonVisuals();
    }
}
