using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;
/// <summary>Verification access to <see cref="InstalledRoomObjectMaridiaElevatubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectMaridiaElevatubeVisualsAccess
{
    /// <summary>Builds the elevatube's stock visual from its single block.</summary>
    extension(InstalledRoomObjectMaridiaElevatubeVisuals)
    {
        /// <summary>Builds the stock elevatube appearance from its single block's terrain.</summary>
        internal static InstalledRoomObjectMaridiaElevatubeVisuals Stock() => RoomObjectMaridiaElevatubeVisualFiles.Create(
            [new RoomObjectVisualEntry(
                RoomObjectMaridiaElevatubeVisualFiles.VisualId,
                [MaridiaElevatubeObjectDefinitions.Terrain.Appearance])]);
    }
}
