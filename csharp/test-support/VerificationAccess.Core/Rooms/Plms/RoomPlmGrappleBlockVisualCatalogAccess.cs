using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmGrappleBlockVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmGrappleBlockVisualCatalogAccess
{
    /// <summary>Creates an uninitialized grapple-block art fixture for PLM binding checks.</summary>
    extension(RoomPlmGrappleBlockVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmGrappleBlockVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmGrappleBlockVisuals();
    }
}
