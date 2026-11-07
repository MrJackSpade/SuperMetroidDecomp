using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectGrappleBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectGrappleBlockVisualsAccess
{
    /// <summary>Creates an uninitialized grapple-block art fixture for PLM binding checks.</summary>
    extension(InstalledRoomObjectGrappleBlockVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomObjectGrappleBlockVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectGrappleBlockVisuals();
    }
}
