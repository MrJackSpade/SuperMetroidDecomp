using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="InstalledRoomPlmGrappleBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmGrappleBlockVisualsAccess
{
    /// <summary>Creates an uninitialized grapple-block art fixture for PLM binding checks.</summary>
    extension(InstalledRoomPlmGrappleBlockVisuals)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static InstalledRoomPlmGrappleBlockVisuals Stock() => RepositoryInstallation.Installation.LoadRoomPlmGrappleBlockVisuals();
    }
}
