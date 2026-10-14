using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmEyeDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmEyeDoorVisualCatalogAccess
{
    /// <summary>Creates an uninitialized eye-door art fixture for PLM binding checks.</summary>
    extension(RoomPlmEyeDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmEyeDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmEyeDoorVisuals();
    }
}
