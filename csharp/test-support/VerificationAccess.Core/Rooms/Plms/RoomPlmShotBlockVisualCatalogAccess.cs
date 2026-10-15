using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmShotBlockVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmShotBlockVisualCatalogAccess
{
    /// <summary>Uses native shot-block draws as the stock visual selection without an installed override.</summary>
    extension(RoomPlmShotBlockVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmShotBlockVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmShotBlockVisuals();
    }
}
