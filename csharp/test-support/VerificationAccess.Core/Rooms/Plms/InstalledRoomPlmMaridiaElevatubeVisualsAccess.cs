namespace SuperMetroid.Core.Rooms.Plms;
/// <summary>Verification access to <see cref="InstalledRoomPlmMaridiaElevatubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmMaridiaElevatubeVisualsAccess
{
    /// <summary>Builds the elevatube's stock visual from its single physical level word.</summary>
    extension(InstalledRoomPlmMaridiaElevatubeVisuals)
    {
        /// <summary>Builds the stock elevatube appearance from its single physical level word.</summary>
        internal static InstalledRoomPlmMaridiaElevatubeVisuals Stock() => new(
            [new RoomPlmVisualEntry(
                MaridiaElevatubePlmDefinitions.VisualId,
                [MaridiaElevatubePlmDefinitions.PhysicalWord.VisualWord])]);
    }
}
