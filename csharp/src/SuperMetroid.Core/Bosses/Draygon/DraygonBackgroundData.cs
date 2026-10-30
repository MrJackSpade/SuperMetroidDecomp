namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Where Draygon's body image is placed on his body layer, and the bands of lines that layer shows on.</summary>
internal static class DraygonBackgroundData
{
    /// <summary>The body layer shows on no line while the body's screen X is below -64 or at or past 320.</summary>
    public const int MinimumScreenX = -64, MaximumScreenX = 320;
    /// <summary>The body layer shows on no line while the body's screen Y is below -16 or at or past 304.</summary>
    public const int MinimumScreenY = -16, MaximumScreenY = 304;
    /// <summary>A body above screen Y 40 shows the top band, one above 192 the whole gameplay area, and lower ones the bottom band.</summary>
    public const int MiddleScreenY = 40, BottomScreenY = 192;
    /// <summary>The top band runs from below the HUD to line 96.</summary>
    public const int TopBandEnd = 96;
    /// <summary>The bottom band runs from line 128 to the bottom of the screen.</summary>
    public const int BottomBandStart = 128;
    /// <summary>$A5:9342, Draygon_Func_36: horizontal artwork origin subtracted after camera minus body X.</summary>
    public const int HorizontalOrigin = 450;

    /// <summary>$A5:9342, Draygon_Func_36: vertical artwork origin subtracted after camera minus body Y.</summary>
    public const int VerticalOrigin = 192;
}
