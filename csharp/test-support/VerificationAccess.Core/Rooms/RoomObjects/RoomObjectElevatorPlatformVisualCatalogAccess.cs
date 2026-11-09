using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectElevatorPlatformVisualCatalog"/> members production does not use.</summary>
internal static class RoomObjectElevatorPlatformVisualCatalogAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(RoomObjectElevatorPlatformVisualCatalog)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static RoomObjectElevatorPlatformVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomObjectElevatorPlatformVisuals();
    }
}
