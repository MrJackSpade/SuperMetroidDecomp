namespace SuperMetroid.Core.Assets;

/// <summary>Tile anchors for the expanded core, lobes, and detached blast tips.</summary>
internal static class CeresStationBlastAtlas
{
    /// <summary>Tile B0, large core quadrant of native frame 8C:9930.</summary>
    internal const int ExpandedCore = 0xb0;
    /// <summary>Tile B2, vertical tip; the horizontal tip occupies the next atlas row.</summary>
    internal const int ExpandedTips = 0xb2;
    /// <summary>Tile B3, first of two adjacent large lobe drawings in 8C:996E.</summary>
    internal const int SplitLobes = 0xb3;
    /// <summary>Tile B7, horizontal peripheral tip of native frame 8C:9998.</summary>
    internal const int DetachedHorizontalTips = 0xb7;
    /// <summary>Tile B8, large detached diagonal quadrant of native frame 8C:9998.</summary>
    internal const int DetachedCore = 0xb8;
    /// <summary>Tile BB, vertical peripheral tip of native frame 8C:9998.</summary>
    internal const int DetachedVerticalTips = 0xbb;
}
