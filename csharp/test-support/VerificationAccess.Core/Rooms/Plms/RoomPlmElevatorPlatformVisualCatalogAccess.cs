using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmElevatorPlatformVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmElevatorPlatformVisualCatalogAccess
{
    /// <summary>Creates an uninitialized elevator-platform art fixture for PLM binding checks.</summary>
    extension(RoomPlmElevatorPlatformVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmElevatorPlatformVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmElevatorPlatformVisuals();
    }
}
