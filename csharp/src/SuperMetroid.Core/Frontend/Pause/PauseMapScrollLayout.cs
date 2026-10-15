namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Pause-specific bounds from Handle_MapScrollArrows ($82:B934).</summary>
internal static class PauseMapScrollLayout
{
    /// <summary>$82:B9A0-B9BE pause arrow anchors are 24 pixels below the matching $81:AF32 file-select records.</summary>
    public const int ArrowVerticalOffset = 24;

    /// <summary>$82:B942: leftmost visible map margin.</summary>
    public const int LeftMargin = 24;
    /// <summary>$82:B954–B958: rightmost visible map coordinate, 256 minus 24.</summary>
    public const int RightMargin = 232;
    /// <summary>$82:B96A: upper map-holder coordinate.</summary>
    public const int TopMargin = 56;
    /// <summary>$82:B97C: lower scroll cutoff.</summary>
    public const int BottomMargin = 177;
}
