namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Pause-specific bounds of the map's scroll arrows.</summary>
internal static class PauseMapScrollLayout
{
    /// <summary>Pause arrow anchors are 24 pixels below the matching file-select records.</summary>
    public const int ArrowVerticalOffset = 24;

    /// <summary>Leftmost visible map margin.</summary>
    public const int LeftMargin = 24;
    /// <summary>Rightmost visible map coordinate, 256 minus 24.</summary>
    public const int RightMargin = 232;
    /// <summary>Upper map-holder coordinate.</summary>
    public const int TopMargin = 56;
    /// <summary>Lower scroll cutoff.</summary>
    public const int BottomMargin = 177;
}
