namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Tile anchors for the expanded core, lobes, and detached blast tips.</summary>
internal static class CeresStationBlastCellDefinitions
{
    /// <summary>Tile B0, large core quadrant of native frame 8C:9930.</summary>
    internal const int ExpandedCore = 176;
    /// <summary>Tile B2, vertical tip; the horizontal tip occupies the next atlas row.</summary>
    internal const int ExpandedTips = 178;
    /// <summary>Tile B3, first of two adjacent large lobe drawings in 8C:996E.</summary>
    internal const int SplitLobes = 179;
    /// <summary>Tile B7, horizontal peripheral tip of native frame 8C:9998.</summary>
    internal const int DetachedHorizontalTips = 183;
    /// <summary>Tile B8, large detached diagonal quadrant of native frame 8C:9998.</summary>
    internal const int DetachedCore = 184;
    /// <summary>Tile BB, vertical peripheral tip of native frame 8C:9998.</summary>
    internal const int DetachedVerticalTips = 187;
}
