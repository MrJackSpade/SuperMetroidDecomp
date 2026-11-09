using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectGrappleBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectGrappleBlockVisualsAccess
{
    /// <summary>The catalog the repository installation holds.</summary>
    extension(InstalledRoomObjectGrappleBlockVisuals)
    {
        /// <summary>Every frame's picture as the repository installation holds it.</summary>
        internal static InstalledRoomObjectGrappleBlockVisuals Stock() => RepositoryInstallation.Installation.LoadRoomObjectGrappleBlockVisuals();
    }
}
