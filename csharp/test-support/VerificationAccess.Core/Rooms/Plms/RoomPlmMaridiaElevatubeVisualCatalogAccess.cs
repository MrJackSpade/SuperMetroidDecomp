namespace SuperMetroid.Core.Rooms.Plms;
/// <summary>Verification access to <see cref="RoomPlmMaridiaElevatubeVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMaridiaElevatubeVisualCatalogAccess
{
    /// <summary>Builds the elevatube's stock visual from its single physical level word.</summary>
    extension(RoomPlmMaridiaElevatubeVisualCatalog)
    {
        /// <summary>Builds the stock elevatube appearance from its single physical level word.</summary>
        internal static RoomPlmMaridiaElevatubeVisualCatalog Stock() => new(
            [new RoomPlmVisualEntry(
                MaridiaElevatubePlmDefinitions.VisualId,
                [MaridiaElevatubePlmDefinitions.PhysicalWord.VisualWord])]);
    }
}
