using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectElevatorPlatformVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectElevatorPlatformVisualCatalogAccess
{
    /// <summary>Creates an uninitialized elevator-platform art fixture for PLM binding checks.</summary>
    extension(RoomObjectElevatorPlatformVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomObjectElevatorPlatformVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectElevatorPlatformVisuals();
    }
}
