using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmNoobTubeVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmNoobTubeVisualCatalogAccess
{
    /// <summary>Leaves Noob Tube stock cells calculated while allowing custom breakage frames.</summary>
    extension(RoomPlmNoobTubeVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmNoobTubeVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmNoobTubeVisuals();
    }
}
